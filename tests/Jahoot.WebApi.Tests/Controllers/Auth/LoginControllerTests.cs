using System.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        _userRepositoryMock.Verify(repo => repo.UpdateUserAsync(It.IsAny<User>()), Times.Never);
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
            PasswordHash = PasswordUtils.HashPasswordWithSalt("password"),
            Roles = [Role.Student]
        };
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(loginRequest.Email)).ReturnsAsync(user);

        IActionResult result = await _loginController.Login(loginRequest);

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        _userRepositoryMock.Verify(repo => repo.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        _loginAttemptServiceMock.Verify(
                                        s => s.RecordFailedLoginAttempt(loginRequest.Email, It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task Login_CorrectCredentials_ReturnsJwtAndUpdatesLastLogin()
    {
        LoginRequest loginRequest = new() { Email = "test@example.com", Password = "password" };
        User user = new()
        {
            UserId = 1,
            Name = "Test User",
            Email = loginRequest.Email,
            PasswordHash = PasswordUtils.HashPasswordWithSalt("password"),
            Roles = [Role.Student],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastLogin = null
        };
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(loginRequest.Email)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.UpdateUserAsync(It.IsAny<User>()))
                           .Callback<User>(u => user.LastLogin = u.LastLogin)
                           .Returns(Task.CompletedTask);

        IActionResult result = await _loginController.Login(loginRequest);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        OkObjectResult okResult = (OkObjectResult)result;
        string? tokenString = okResult.Value?.GetType().GetProperty("Token")?.GetValue(okResult.Value, null) as string;
        Assert.That(tokenString, Is.Not.Null);

        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken jsonToken = handler.ReadJwtToken(tokenString);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(jsonToken.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == nameof(Role.Student)), Is.True);
            Assert.That(user.LastLogin, Is.Not.Null);
        }
        _userRepositoryMock.Verify(repo => repo.UpdateUserAsync(user), Times.Once);
    }

    [Test]
    public async Task Login_InvalidModelState_ReturnsBadRequest()
    {
        LoginRequest loginRequest = new() { Email = "invalid-email", Password = "password" };

        // Manually add a model error to simulate an invalid model state
        _loginController.ModelState.AddModelError("Email", "The Email field is not a valid email address.");

        IActionResult result = await _loginController.Login(loginRequest);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        _userRepositoryMock.Verify(repo => repo.UpdateUserAsync(It.IsAny<User>()), Times.Never);
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
