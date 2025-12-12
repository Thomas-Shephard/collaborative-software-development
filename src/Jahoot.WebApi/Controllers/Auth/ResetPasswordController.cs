using System.Data;
using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.Core.Utils;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Auth;

[ApiController]
[Route("api/auth/reset-password")]
public class ResetPasswordController(IDbConnection connection, IUserRepository userRepository, IPasswordResetRepository passwordResetRepository) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestModel requestModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        using IDbTransaction transaction = connection.BeginTransaction();
        try
        {
            User? user = await userRepository.GetUserByEmailAsync(requestModel.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Failed to reset password." });
            }

            PasswordResetToken? passwordResetToken = await passwordResetRepository.GetPasswordResetTokenByEmail(requestModel.Email);
            if (passwordResetToken is null || passwordResetToken.IsUsed || passwordResetToken.IsRevoked || passwordResetToken.Expiration <= DateTime.UtcNow
                || !PasswordUtils.VerifyPassword(requestModel.Token, passwordResetToken.TokenHash))
            {
                return BadRequest(new { message = "Failed to reset password." });
            }

            string newHashedPassword = PasswordUtils.HashPasswordWithSalt(requestModel.NewPassword);
            user.PasswordHash = newHashedPassword;
            passwordResetToken.IsUsed = true;

            await userRepository.UpdateUserAsync(user);
            await passwordResetRepository.UpdatePasswordResetTokenAsync(passwordResetToken);

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
