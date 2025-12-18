using System.Data;
using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.Core.Utils;
using Jahoot.WebApi.Attributes;
using Jahoot.WebApi.Repositories;
using Jahoot.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Auth;

[ApiController]
[Route("api/auth/reset-password")]
[Tags("Auth")]
public class ResetPasswordController(IDbConnection connection, IUserRepository userRepository, IPasswordResetRepository passwordResetRepository, ISecurityLockoutService securityLockoutService) : ControllerBase
{
    private const string FailMessage = "Failed to reset password.";

    [HttpPost]
    [SecurityLockout]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestModel requestModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        User? user = await userRepository.GetUserByEmailAsync(requestModel.Email);
        if (user is null)
        {
            return BadRequest(new { message = FailMessage });
        }

        using IDbTransaction transaction = connection.BeginTransaction();
        try
        {
            PasswordResetToken? passwordResetToken = await passwordResetRepository.GetPasswordResetTokenByEmail(requestModel.Email, transaction);
            if (passwordResetToken is null || passwordResetToken.IsUsed || passwordResetToken.IsRevoked || passwordResetToken.Expiration <= DateTime.UtcNow
                || !PasswordUtils.VerifyPassword(requestModel.Token, passwordResetToken.TokenHash))
            {
                transaction.Rollback();
                return BadRequest(new { message = FailMessage });
            }

            string newHashedPassword = PasswordUtils.HashPasswordWithSalt(requestModel.NewPassword);
            user.PasswordHash = newHashedPassword;
            passwordResetToken.IsUsed = true;

            await userRepository.UpdateUserAsync(user, transaction);
            await passwordResetRepository.UpdatePasswordResetTokenAsync(passwordResetToken, transaction);

            string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            await securityLockoutService.ResetAttempts($"IP:{ipAddress}", $"Email:{requestModel.Email}");

            transaction.Commit();
            return Ok(new { message = "Password has been reset successfully." });
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}