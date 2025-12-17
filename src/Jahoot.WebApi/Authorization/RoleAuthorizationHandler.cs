using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Jahoot.Core.Models;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace Jahoot.WebApi.Authorization;

public class RoleAuthorizationHandler(IServiceScopeFactory serviceScopeFactory) : AuthorizationHandler<RoleRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        if (context.User.Identity is not { IsAuthenticated: true })
        {
            return;
        }

        string? userIdString = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                               context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return;
        }

        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IUserRepository userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        User? user = await userRepository.GetUserByIdAsync(userId);

        if (user is null || user.IsDisabled)
        {
            return;
        }

        if (user.Roles.Contains(requirement.Role))
        {
            context.Succeed(requirement);
        }
    }
}
