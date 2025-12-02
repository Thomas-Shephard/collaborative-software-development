using Jahoot.Core.Models;

namespace Jahoot.Display.Services;

public class LoginResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IAuthService
{
    Task<LoginResult> Login(LoginRequest loginRequest);

    Task Logout();
}
