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
    private Mock<HttpContext> _httpContextMock;
    private Mock<ILoginAttemptService> _loginAttemptServiceMock;
    private LoginController _loginController;
    private Mock<IUserRepository> _userRepositoryMock;

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
        _httpContextMock = new Mock<HttpContext>();
        Mock<ConnectionInfo> connection = new();
        connection.Setup(c => c.RemoteIpAddress).Returns(new IPAddress(new byte[] { 127, 0, 0, 1 }));
        _httpContextMock.Setup(c => c.Connection).Returns(connection.Object);
        _loginController.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContextMock.Object
        };
    }

    [Test]
    public async Task Login_LockedOut_ReturnsTooManyRequests()
    {
        LoginRequest loginRequest = new() { Email = "test@example.com", Password = "password" };
        _loginAttemptServiceMock.Setup(s => s.IsLockedOut(loginRequest.Email, It.IsAny<string>()))
                                .ReturnsAsync(true);

        IActionResult result = await _loginController.Login(loginRequest);

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
            UserId = 1,
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
            UserId = 1,
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
    public async Task Login_CorrectCredentials_UpdatesLastLogin()
    {
        LoginRequest loginRequest = new() { Email = "test@example.com", Password = "password" };
        User user = new()
        {
            UserId = 1,
            Name = "Test User",
            Email = loginRequest.Email,
            PasswordHash = PasswordUtils.HashPasswordWithSalt("password")
        };
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(loginRequest.Email)).ReturnsAsync(user);

        DateTime testStartTime = DateTime.UtcNow;
        await _loginController.Login(loginRequest);

        _userRepositoryMock.Verify(
                                   repo => repo.UpdateUserAsync(It.Is<User>(u =>
                                                                                u.UserId == user.UserId &&
                                                                                u.LastLogin.HasValue &&
                                                                                u.LastLogin.Value >= testStartTime)),
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

    [Test]
    public async Task Login_MissingIpAddress_ReturnsBadRequest()
    {
        LoginRequest loginRequest = new() { Email = "test@example.com", Password = "password" };
        Mock<ConnectionInfo> connection = new();
        connection.Setup(c => c.RemoteIpAddress).Returns(() => null);
        _httpContextMock.Setup(c => c.Connection).Returns(connection.Object);

        IActionResult result = await _loginController.Login(loginRequest);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;
        Assert.That(badRequestResult.Value, Is.EqualTo("Could not determine IP address."));
    }
}
