using System.Security.Cryptography;
using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.Core.Utils;
using Jahoot.WebApi.Repositories;
using Jahoot.WebApi.Services.Background;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Auth;

[ApiController]
[Route("api/auth/forgot-password")]
[Tags("Auth")]
public class ForgotPasswordController(IUserRepository userRepository, IPasswordResetRepository passwordResetRepository, IEmailQueue emailQueue) : ControllerBase
{
    private const string SuccessMessage = "If the email exists, a password reset token has been sent.";

    [HttpPost]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestModel requestModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        User? user = await userRepository.GetUserByEmailAsync(requestModel.Email);

        // Always perform token generation and hashing to mitigate timing attacks (CPU bound)
        string token = RandomNumberGenerator.GetInt32(0, 999_999).ToString("D6");
        string hashedToken = PasswordUtils.HashPasswordWithSalt(token);

        if (user == null)
        {
            return Ok(new { message = SuccessMessage });
        }

        await passwordResetRepository.CreateTokenAsync(user.UserId, hashedToken);

        await emailQueue.QueueBackgroundEmailAsync(new EmailMessage
        {
            To = user.Email,
            Subject = "Reset Your Jahoot Password",
            Title = "Password Reset",
            Body = $"Use the code {token} to reset your password. You have 15 minutes until the code expires."
        });

        return Ok(new { message = SuccessMessage });
    }
}
