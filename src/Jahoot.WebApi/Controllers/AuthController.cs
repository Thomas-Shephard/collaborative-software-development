using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Jahoot.Core.Models;
using Jahoot.Core.Utils;
using Jahoot.WebApi.Repositories;
using Jahoot.WebApi.Services;
using Jahoot.WebApi.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Jahoot.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IUserRepository userRepository, JwtSettings jwtSettings, ITokenDenyService tokenDenyService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        User? user = await userRepository.GetUserByEmailAsync(request.Email);

        // To prevent timing attacks, the code path should be the same whether the user is found or not.
        // The password verification will then fail, but it will have taken the same amount of time.
        bool isPasswordCorrect = PasswordUtils.VerifyPassword(request.Password, user?.PasswordHash);

        if (!isPasswordCorrect)
        {
            return Unauthorized();
        }

        DateTime loginTime = DateTime.UtcNow;

        user!.LastLogin = loginTime;
        await userRepository.UpdateUserAsync(user);

        // Generate JWT
        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(jwtSettings.Secret));
        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.Name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        DateTime expires = loginTime.AddDays(7);
        JwtSecurityToken token = new(
                                     jwtSettings.Issuer,
                                     jwtSettings.Audience,
                                     claims,
                                     loginTime,
                                     expires,
                                     signingCredentials: credentials);

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new { Token = tokenString });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        string? jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        if (jti is null)
        {
            return BadRequest("Token does not contain a JTI claim.");
        }

        string? expiresValue = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
        DateTime jwtExpiryTime = long.TryParse(expiresValue, out long expiresSeconds)
            ? DateTimeOffset.FromUnixTimeSeconds(expiresSeconds).UtcDateTime
            : DateTime.MaxValue;

        await tokenDenyService.DenyAsync(jti, jwtExpiryTime);

        return Ok();
    }
}
