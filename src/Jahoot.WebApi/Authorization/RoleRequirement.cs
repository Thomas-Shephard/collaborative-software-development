using Jahoot.Core.Models;
using Microsoft.AspNetCore.Authorization;

namespace Jahoot.WebApi.Authorization;

public class RoleRequirement(Role role) : IAuthorizationRequirement
{
    public Role Role { get; } = role;
}
