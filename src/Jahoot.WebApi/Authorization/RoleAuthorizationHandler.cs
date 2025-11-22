using System.Security.Claims;
using Jahoot.Core.Models;
using Microsoft.AspNetCore.Authorization;

namespace Jahoot.WebApi.Authorization;

public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        if (context.User.Identity is { IsAuthenticated: false })
        {
            return Task.CompletedTask;
        }

        List<Role> roles = context.User.FindAll(ClaimTypes.Role).Select(claim => Enum.Parse<Role>(claim.Value)).ToList();

        if (roles.Contains(requirement.Role) || (requirement.Role == Role.Lecturer && roles.Contains(Role.Admin)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
