using System.Net;
using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.Core.Utils;
using Jahoot.WebApi.Controllers.Auth;
using Jahoot.WebApi.Repositories;
using Jahoot.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Data;

namespace Jahoot.WebApi.Tests.Controllers.Auth;

public class LoginControllerTests
{
    private Mock<HttpContext> _httpContextMock;
    private Mock<ILoginAttemptService> _loginAttemptServiceMock;
    private LoginController _loginController;
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<ITokenService> _tokenServiceMock;

    [SetUp]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _loginAttemptServiceMock = new Mock<ILoginAttemptService>();
        _tokenServiceMock = new Mock<ITokenService>();

        _loginController = new LoginController(_userRepositoryMock.Object, _loginAttemptServiceMock.Object, _tokenServiceMock.Object);
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
        LoginRequestModel loginRequestModel = new() { Email = "test@example.com", Password = "password" };
        _loginAttemptServiceMock.Setup(s => s.IsLockedOut(loginRequestModel.Email, It.IsAny<string>()))
                                .ReturnsAsync(true);

        IActionResult result = await _loginController.Login(loginRequestModel);

        Assert.That(result, Is.TypeOf<ObjectResult>());
        ObjectResult objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(429));
    }

    [Test]
    public async Task Login_IncorrectEmail_ReturnsUnauthorized()
    {
        LoginRequestModel loginRequestModel = new() { Email = "test@example.com", Password = "password" };
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(loginRequestModel.Email)).ReturnsAsync(() => null);

        IActionResult result = await _loginController.Login(loginRequestModel);

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        _userRepositoryMock.Verify(repo => repo.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        _loginAttemptServiceMock.Verify(
                                        s => s.RecordFailedLoginAttempt(loginRequestModel.Email, It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task Login_IncorrectPassword_ReturnsUnauthorized()
    {
        LoginRequestModel loginRequestModel = new() { Email = "test@example.com", Password = "wrong-password" };
        User user = new()
        {
            UserId = 1,
            Name = "Test User",
            Email = loginRequestModel.Email,
            PasswordHash = PasswordUtils.HashPasswordWithSalt("password"),
            Roles = [Role.Student]
        };
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(loginRequestModel.Email)).ReturnsAsync(user);

        IActionResult result = await _loginController.Login(loginRequestModel);

        Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        _userRepositoryMock.Verify(repo => repo.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        _loginAttemptServiceMock.Verify(
                                        s => s.RecordFailedLoginAttempt(loginRequestModel.Email, It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task Login_CorrectCredentials_ReturnsJwtAndUpdatesLastLogin()
    {
        LoginRequestModel loginRequestModel = new() { Email = "test@example.com", Password = "password" };
        User user = new()
        {
            UserId = 1,
            Name = "Test User",
            Email = loginRequestModel.Email,
            PasswordHash = PasswordUtils.HashPasswordWithSalt("password"),
            Roles = [Role.Student],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastLogin = null
        };
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(loginRequestModel.Email)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.UpdateUserAsync(It.IsAny<User>(), It.IsAny<IDbTransaction?>()))
                           .Callback<User, IDbTransaction?>((u, t) => user.LastLogin = u.LastLogin)
                           .Returns(Task.CompletedTask);

        const string expectedToken = "mocked_jwt_token";
        _tokenServiceMock.Setup(s => s.GenerateToken(user)).Returns(expectedToken);

        IActionResult result = await _loginController.Login(loginRequestModel);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        OkObjectResult okResult = (OkObjectResult)result;
        string? tokenString = okResult.Value?.GetType().GetProperty("Token")?.GetValue(okResult.Value, null) as string;
        Assert.That(tokenString, Is.EqualTo(expectedToken));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(user.LastLogin, Is.Not.Null);
        }
        _userRepositoryMock.Verify(repo => repo.UpdateUserAsync(user), Times.Once);
        _tokenServiceMock.Verify(s => s.GenerateToken(user), Times.Once);
    }

    [Test]
    public async Task Login_InvalidModelState_ReturnsBadRequest()
    {
        LoginRequestModel loginRequestModel = new() { Email = "invalid-email", Password = "password" };

        // Manually add a model error to simulate an invalid model state
        _loginController.ModelState.AddModelError("Email", "The Email field is not a valid email address.");

        IActionResult result = await _loginController.Login(loginRequestModel);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        _userRepositoryMock.Verify(repo => repo.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task Login_MissingIpAddress_ReturnsBadRequest()
    {
        LoginRequestModel loginRequestModel = new() { Email = "test@example.com", Password = "password" };
        Mock<ConnectionInfo> connection = new();
        connection.Setup(c => c.RemoteIpAddress).Returns(() => null);
        _httpContextMock.Setup(c => c.Connection).Returns(connection.Object);

        IActionResult result = await _loginController.Login(loginRequestModel);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;
        Assert.That(badRequestResult.Value, Is.EqualTo("Could not determine IP address."));
    }
}
