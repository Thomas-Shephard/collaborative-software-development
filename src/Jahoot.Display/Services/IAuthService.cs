using Jahoot.Core.Models;
using System.Collections.Generic;

namespace Jahoot.Display.Services;

public class LoginResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<Role> UserRoles { get; set; } = new();
}

public interface IAuthService
{
    Task<LoginResult> Login(LoginRequest loginRequest);

    Task Logout();
}
