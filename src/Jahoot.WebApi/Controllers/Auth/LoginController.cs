using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.Core.Utils;
using Jahoot.WebApi.Repositories;
using Jahoot.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Auth;

[ApiController]
[Route("api/auth/login")]
public class LoginController(IUserRepository userRepository, ILoginAttemptService loginAttemptService, ITokenService tokenService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel requestModel)
    {
        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        if (string.IsNullOrEmpty(ipAddress))
        {
            return BadRequest("Could not determine IP address.");
        }

        if (await loginAttemptService.IsLockedOut(requestModel.Email, ipAddress))
        {
            return StatusCode(429, "Too many failed login attempts. Please try again later.");
        }

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
            await loginAttemptService.RecordFailedLoginAttempt(requestModel.Email, ipAddress);
            return Unauthorized();
        }

        await loginAttemptService.ResetFailedLoginAttempts(requestModel.Email, ipAddress);

        DateTime loginTime = DateTime.UtcNow;

        user!.LastLogin = loginTime;
        await userRepository.UpdateUserAsync(user);

        string tokenString = tokenService.GenerateToken(user);

        return Ok(new { Token = tokenString });
    }
}
