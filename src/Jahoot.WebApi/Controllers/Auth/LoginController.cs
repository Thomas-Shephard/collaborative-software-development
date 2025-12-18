using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.Core.Utils;
using Jahoot.WebApi.Attributes;
using Jahoot.WebApi.Repositories;
using Jahoot.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Auth;

[ApiController]
[Route("api/auth/login")]
[Tags("Auth")]
public class LoginController(IUserRepository userRepository, ISecurityLockoutService securityLockoutService, ITokenService tokenService) : ControllerBase
{
    [HttpPost]
    [SecurityLockout]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel requestModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        User? user = await userRepository.GetUserByEmailAsync(requestModel.Email);

        // To prevent timing attacks, the code path should be the same whether the user is found or not.
        // The password verification will then fail, but it will have taken the same amount of time.
        bool isPasswordCorrect = PasswordUtils.VerifyPassword(requestModel.Password, user?.PasswordHash);

        if (!isPasswordCorrect)
        {
            return Unauthorized();
        }

        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        await securityLockoutService.ResetAttempts($"IP:{ipAddress}", $"Email:{requestModel.Email}");

        if (user!.Roles.Count == 0)
        {
            return StatusCode(403, "Your account is not yet approved. Please contact your lecturer or administrator for access.");
        }

        DateTime loginTime = DateTime.UtcNow;

        user.LastLogin = loginTime;
        await userRepository.UpdateUserAsync(user);

        string tokenString = tokenService.GenerateToken(user);

        return Ok(new { Token = tokenString });
    }
}
