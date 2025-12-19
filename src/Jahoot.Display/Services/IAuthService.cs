using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using System.Collections.Generic;

namespace Jahoot.Display.Services;

public class Result
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<Role> UserRoles { get; set; } = new();
}


public interface IAuthService
{
    Task<Result> Login(LoginRequestModel loginRequest);
    Task<Result> Register(CreateStudentRequestModel registerRequest);
    Task<Result> ForgotPassword(string email);
    Task<Result> ResetPassword(string email, string token, string newPassword);

    Task Logout();
}
