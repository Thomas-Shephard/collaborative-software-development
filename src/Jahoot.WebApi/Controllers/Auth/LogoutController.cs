using System.IdentityModel.Tokens.Jwt;
using Jahoot.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Auth;

/// <summary>
/// This is the controller for logging out.
/// </summary>
[ApiController]
[Route("api/auth/logout")]
public class LogoutController(ITokenDenyService tokenDenyService) : ControllerBase
{
    /// <summary>
    /// This is the method that logs a user out.
    /// </summary>
    /// <returns>An OK result if the logout is successful.</returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        string? jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        if (jti is null)
        {
            return BadRequest("Token does not contain a JTI claim.");
        }

        string? expiresValue = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
        if (!long.TryParse(expiresValue, out long expiresSeconds))
        {
            return BadRequest("Token does not contain a valid EXP claim.");
        }

        DateTime jwtExpiryTime = DateTimeOffset.FromUnixTimeSeconds(expiresSeconds).UtcDateTime;

        await tokenDenyService.DenyAsync(jti, jwtExpiryTime);

        return Ok();
    }
}
