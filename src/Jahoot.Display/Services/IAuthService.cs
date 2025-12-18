using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Display.Services;



public interface IAuthService
{
    Task<Result> Login(LoginRequestModel loginRequest);
    Task<Result> Register(CreateStudentRequestModel registerRequest);
    Task<Result> ForgotPassword(string email);
    Task<Result> ResetPassword(string email, string token, string newPassword);

    Task Logout();
}
