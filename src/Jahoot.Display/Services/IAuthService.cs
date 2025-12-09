using Jahoot.Core.Models.Requests;

namespace Jahoot.Display.Services;

public class LoginResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class RegisterResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IAuthService
{
    Task<LoginResult> Login(LoginRequestModel loginRequest);
    Task<RegisterResult> Register(StudentRegistrationRequestModel registerRequest);

    Task Logout();
}
