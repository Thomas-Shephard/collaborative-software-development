using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Jahoot.Core.Models;
using Jahoot.Core.Utils;
using Jahoot.WebApi.Repositories;
using Jahoot.WebApi.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Jahoot.WebApi.Controllers.Auth;

[ApiController]
[Route("api/auth/login")]
public class LoginController(IUserRepository userRepository, JwtSettings jwtSettings) : ControllerBase
{
    [HttpPost]
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
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
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
                                     credentials);

        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new { Token = tokenString });
    }
}
