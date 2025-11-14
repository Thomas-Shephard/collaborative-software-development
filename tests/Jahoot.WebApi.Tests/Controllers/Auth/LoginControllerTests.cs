using Jahoot.Core.Models;
using Jahoot.Core.Utils;
using Jahoot.WebApi.Controllers.Auth;
using Jahoot.WebApi.Repositories;
using Jahoot.WebApi.Settings;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Auth;

public class LoginControllerTests
{
    private LoginController _loginController;
    private Mock<IUserRepository> _userRepositoryMock;

    [SetUp]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        JwtSettings jwtSettings = new()
        {
            Secret = "a-very-secure-secret-key-that-is-long-enough",
            Issuer = "Jahoot",
            Audience = "Jahoot"
        };
        _loginController = new LoginController(_userRepositoryMock.Object, jwtSettings);
    }

    [Test]
    public async Task Login_IncorrectEmail_ReturnsUnauthorized()
    {
        LoginRequest loginRequest = new() { Email = "test@example.com", Password = "password" };
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(loginRequest.Email)).ReturnsAsync(() => null);

        IActionResult result = await _loginController.Login(loginRequest);

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

        IActionResult result = await _loginController.Login(loginRequest);

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

        IActionResult result = await _loginController.Login(loginRequest);

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
        _loginController.ModelState.AddModelError("Email", "The Email field is not a valid email address.");

        IActionResult result = await _loginController.Login(loginRequest);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }
}
