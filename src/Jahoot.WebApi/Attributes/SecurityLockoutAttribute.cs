using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Jahoot.WebApi.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class SecurityLockoutAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ISecurityLockoutService? securityLockoutService = context.HttpContext.RequestServices.GetService<ISecurityLockoutService>();

        if (securityLockoutService == null)
        {
            await next();
            return;
        }

        string? ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrEmpty(ipAddress))
        {
            context.Result = new BadRequestObjectResult("Could not determine IP address.");
            return;
        }

        string ipKey = $"IP:{ipAddress}";
        string? emailKey = null;

        IEmailRequest? emailRequest = context.ActionArguments.Values.OfType<IEmailRequest>().FirstOrDefault();
        if (emailRequest != null)
        {
            emailKey = $"Email:{emailRequest.Email}";
        }

        string[] keys = emailKey != null ? [ipKey, emailKey] : [ipKey];

        if (await securityLockoutService.IsLockedOut(keys))
        {
            context.Result = new ObjectResult("Too many attempts. Please try again later.")
            {
                StatusCode = 429
            };
            return;
        }

        await securityLockoutService.RecordFailure(keys);

        await next();
    }
}
