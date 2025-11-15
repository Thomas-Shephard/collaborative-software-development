using System.Net;
using Jahoot.Core.Models;
using Jahoot.Core.Utils;
using Jahoot.WebApi.Controllers.Auth;
using Jahoot.WebApi.Repositories;
using Jahoot.WebApi.Services;
using Jahoot.WebApi.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Auth;

public class LoginControllerTests
{
    private Mock<ILoginAttemptService> _loginAttemptServiceMock = null!;
    private LoginController _loginController = null!;
    private Mock<IUserRepository> _userRepositoryMock = null!;

    [SetUp]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _loginAttemptServiceMock = new Mock<ILoginAttemptService>();
        JwtSettings jwtSettings = new()
        {
            Secret = "a-very-secure-secret-key-that-is-long-enough",
            Issuer = "Jahoot",
            Audience = "Jahoot"
        };
        _loginController = new LoginController(_userRepositoryMock.Object, jwtSettings, _loginAttemptServiceMock.Object);
        Mock<HttpContext> httpContext = new();
        Mock<ConnectionInfo> connection = new();
        connection.Setup(c => c.RemoteIpAddress).Returns(new IPAddress(new byte[] { 127, 0, 0, 1 }));
        httpContext.Setup(c => c.Connection).Returns(connection.Object);
        _loginController.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext.Object
        };
    }

    [Test]
    public async Task Login_LockedOut_ReturnsTooManyRequests()
    {
        // Arrange
        LoginRequest loginRequest = new() { Email = "test@example.com", Password = "password" };
        _loginAttemptServiceMock.Setup(s => s.IsLockedOut(loginRequest.Email, It.IsAny<string>()))
                                .ReturnsAsync(true);

        // Act
        IActionResult result = await _loginController.Login(loginRequest);

        // Assert
        Assert.That(result, Is.TypeOf<ObjectResult>());
        ObjectResult objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(429));
    }


    [Test]
    public async Task Login_IncorrectEmail_ReturnsUnauthorized()
    {
        LoginRequest loginRequest = new() { Email = "test@example.com", Password = "password" };
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(loginRequest.Email)).ReturnsAsync(() => null);

        IActionResult result = await _loginController.Login(loginRequest);

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        _loginAttemptServiceMock.Verify(
                                        s => s.RecordFailedLoginAttempt(loginRequest.Email, It.IsAny<string>()), Times.Once);
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
        _loginAttemptServiceMock.Verify(
                                        s => s.RecordFailedLoginAttempt(loginRequest.Email, It.IsAny<string>()), Times.Once);
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
        _loginAttemptServiceMock.Verify(s => s.ResetFailedLoginAttempts(loginRequest.Email, It.IsAny<string>()),
                                        Times.Once);
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
