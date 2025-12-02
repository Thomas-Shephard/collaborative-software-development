using System.Security.Cryptography;
using Jahoot.Core.Models;
using Jahoot.WebApi.Repositories;
using Jahoot.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class ForgotPasswordController( IUserRepository userRepository, IPasswordResetRepository passwordResetRepository, IEmailService emailService) : ControllerBase
{
    private const string SuccessMessage = "If the email exists, a password reset token has been sent.";

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        User? user = await userRepository.GetUserByEmailAsync(request.Email);
        if (user == null)
        {
            return Ok(new { message = SuccessMessage });
        }

        // Create a cryptographically secure 6 digit random number
        string token = RandomNumberGenerator.GetInt32(100_000, 999_999).ToString();

        await passwordResetRepository.CreateTokenAsync(user.UserId, token);

        await emailService.SendEmailAsync(
            user.Email,
            "Jahoot Password Reset",
            $"Use the code {token}  to reset your password."
        );

        return Ok(new { message = SuccessMessage });
    }
}
