using System.Security.Claims;
using Jahoot.Core.Models;
using Microsoft.AspNetCore.Authorization;

namespace Jahoot.WebApi.Authorization;

public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        if (context.User.Identity is not { IsAuthenticated: true })
        {
            return Task.CompletedTask;
        }

        List<Role> roles = context.User
                                  .FindAll(ClaimTypes.Role)
                                  .Select(claim => Enum.TryParse(claim.Value, out Role role) ? (Role?)role : null)
                                  .OfType<Role>()
                                  .ToList();

        if (roles.Contains(requirement.Role))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
