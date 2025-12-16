using Jahoot.Core.Models.Requests;

namespace Jahoot.Display.Services;

public class Result
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IAuthService
{
    Task<Result> Login(LoginRequestModel loginRequest);
    Task<Result> Register(CreateStudentRequestModel registerRequest);

    Task Logout();
}
