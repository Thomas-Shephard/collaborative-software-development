using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Jahoot.Core.Models;
using Jahoot.WebApi.Services;
using Jahoot.WebApi.Settings;

namespace Jahoot.WebApi.Tests.Services;

public class TokenServiceTests
{
    private TokenService _tokenService;
    private JwtSettings _jwtSettings;

    [SetUp]
    public void Setup()
    {
        _jwtSettings = new JwtSettings
        {
            Secret = "this-is-a-secure-secret-key-for-testing-purposes-only-32-chars",
            Issuer = "JahootTestIssuer",
            Audience = "JahootTestAudience"
        };
        _tokenService = new TokenService(_jwtSettings);
    }

    [Test]
    public void GenerateToken_ValidUser_ReturnsTokenWithCorrectClaims()
    {
        User user = new()
        {
            UserId = 123,
            Email = "test@example.com",
            Name = "Test User",
            PasswordHash = "hashed_password",
            Roles = [Role.Student, Role.Lecturer]
        };

        string token = _tokenService.GenerateToken(user);

        Assert.That(token, Is.Not.Null.And.Not.Empty);

        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken jsonToken = handler.ReadJwtToken(token);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(jsonToken.Issuer, Is.EqualTo(_jwtSettings.Issuer));
            Assert.That(jsonToken.Audiences.First(), Is.EqualTo(_jwtSettings.Audience));

            Assert.That(jsonToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value, Is.EqualTo(user.UserId.ToString()));
            Assert.That(jsonToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value, Is.EqualTo(user.Email));
            Assert.That(jsonToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Name).Value, Is.EqualTo(user.Name));
            Assert.That(jsonToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value, Is.Not.Null.And.Not.Empty);

            List<string> roles = jsonToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            Assert.That(roles, Contains.Item(nameof(Role.Student)));
            Assert.That(roles, Contains.Item(nameof(Role.Lecturer)));
        }
    }

    [Test]
    public void GenerateToken_ValidUser_ReturnsTokenWithCorrectExpiration()
    {
        User user = new()
        {
            UserId = 1,
            Email = "test@example.com",
            Name = "Test User",
            PasswordHash = "hash",
            Roles = [Role.Student]
        };

        DateTime before = DateTime.UtcNow;
        string token = _tokenService.GenerateToken(user);
        DateTime after = DateTime.UtcNow;

        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken jsonToken = handler.ReadJwtToken(token);

        DateTime expectedExpiry = before.AddDays(7);

        // Check it is close to the expiration time (within 1 second either way)
        Assert.That(jsonToken.ValidTo, Is.EqualTo(expectedExpiry.AddSeconds(-1)).And.LessThanOrEqualTo(after.AddDays(7).AddSeconds(1)));
    }
}
