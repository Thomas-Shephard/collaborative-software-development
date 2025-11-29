using Jahoot.Core.Models;
using Jahoot.Core.Utils;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public class ResetPasswordController(IPasswordResetRepository passwordResetRepository, IUserRepository userRepository) : ControllerBase
{
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        PasswordResetToken? token = await passwordResetRepository.GetTokenByTokenAsync(request.Token);

        if (token is null || token.IsUsed || token.Expiration < DateTime.UtcNow)
        {
            return BadRequest(new { message = "Invalid or expired token." });
        }

        // Suppress nullable warning as SQL ON DELETE CASCADE means that the user must exist
        User user = (await userRepository.GetUserByIdAsync(token.UserId))!;

        user.PasswordHash = PasswordUtils.HashPasswordWithSalt(request.NewPassword);
        await userRepository.UpdateUserAsync(user);

        token.IsUsed = true;
        await passwordResetRepository.UpdateTokenAsync(token);

        return Ok();
    }
}
