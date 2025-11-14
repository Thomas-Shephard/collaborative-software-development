using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Jahoot.Core.Models;
using Jahoot.Core.Utils;
using Jahoot.WebApi.Controllers;
using Jahoot.WebApi.Repositories;
using Jahoot.WebApi.Services;
using Jahoot.WebApi.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers;

public class AuthControllerTests
{
    private AuthController _authController;
    private Mock<ITokenDenyService> _tokenDenyServiceMock;
    private Mock<IUserRepository> _userRepositoryMock;

    [SetUp]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _tokenDenyServiceMock = new Mock<ITokenDenyService>();
        JwtSettings jwtSettings = new()
        {
            Secret = "a-very-secure-secret-key-that-is-long-enough",
            Issuer = "Jahoot",
            Audience = "Jahoot"
        };
        _authController = new AuthController(_userRepositoryMock.Object, jwtSettings, _tokenDenyServiceMock.Object);
    }

    [Test]
    public async Task Login_IncorrectEmail_ReturnsUnauthorized()
    {
        LoginRequest loginRequest = new() { Email = "test@example.com", Password = "password" };
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(loginRequest.Email)).ReturnsAsync(() => null);

        IActionResult result = await _authController.Login(loginRequest);

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
    }

    [Test]
    public async Task Login_IncorrectPassword_ReturnsUnauthorized()
    {
        LoginRequest loginRequest = new() { Email = "test@example.com", Password = "wrong-password" };
        User user = new()
        {
            Id = 1,
            Name = "Test User",
            Email = loginRequest.Email,
            PasswordHash = PasswordUtils.HashPasswordWithSalt("password")
        };
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(loginRequest.Email)).ReturnsAsync(user);

        IActionResult result = await _authController.Login(loginRequest);

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
    }

    [Test]
    public async Task Login_CorrectCredentials_ReturnsJwt()
    {
        LoginRequest loginRequest = new() { Email = "test@example.com", Password = "password" };
        User user = new()
        {
            Id = 1,
            Name = "Test User",
            Email = loginRequest.Email,
            PasswordHash = PasswordUtils.HashPasswordWithSalt("password")
        };
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(loginRequest.Email)).ReturnsAsync(user);

        IActionResult result = await _authController.Login(loginRequest);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        OkObjectResult okResult = (OkObjectResult)result;
        object? token = okResult.Value?.GetType().GetProperty("Token")?.GetValue(okResult.Value, null);
        Assert.That(token, Is.Not.Null);
    }

    [Test]
    public async Task Login_InvalidModelState_ReturnsBadRequest()
    {
        LoginRequest loginRequest = new() { Email = "invalid-email", Password = "password" };

        // Manually add a model error to simulate an invalid model state
        _authController.ModelState.AddModelError("Email", "The Email field is not a valid email address.");

        IActionResult result = await _authController.Login(loginRequest);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Logout_ValidToken_DeniesTokenAndReturnsOk()
    {
        string jti = Guid.NewGuid().ToString();
        long exp = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds();
        ClaimsPrincipal user = new(new ClaimsIdentity([
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(JwtRegisteredClaimNames.Exp, exp.ToString())
        ], "mock"));

        _authController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        IActionResult result = await _authController.Logout();

        Assert.That(result, Is.TypeOf<OkResult>());
        _tokenDenyServiceMock.Verify(tokenDenyService => tokenDenyService.DenyAsync(jti, It.IsAny<DateTime>()), Times.Once);
    }

    [Test]
    public async Task Logout_TokenWithoutExp_DeniesTokenWithMaxExpiry()
    {
        string jti = Guid.NewGuid().ToString();
        ClaimsPrincipal user = new(new ClaimsIdentity([
            new Claim(JwtRegisteredClaimNames.Jti, jti)
            // No Exp claim
        ], "mock"));

        _authController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        IActionResult result = await _authController.Logout();

        Assert.That(result, Is.TypeOf<OkResult>());
        _tokenDenyServiceMock.Verify(tokenDenyService => tokenDenyService.DenyAsync(jti, DateTime.MaxValue), Times.Once);
    }

    [Test]
    public async Task Logout_TokenWithoutJti_ReturnsProblem()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity([
            // No JTI claim
        ], "mock"));

        _authController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        IActionResult result = await _authController.Logout();

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;
        Assert.That(badRequestResult.Value, Is.EqualTo("Token does not contain a JTI claim."));
    }
}
