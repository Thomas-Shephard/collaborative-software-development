using Moq;
using System.Data;
using System.Net;
using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.Core.Utils;
using Jahoot.WebApi.Repositories;
using Jahoot.WebApi.Controllers.Auth;
using Jahoot.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Tests.Controllers.Auth;

public class ResetPasswordControllerTests
{
    private Mock<IDbConnection> _mockConnection;
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IPasswordResetRepository> _mockPasswordResetRepository;
    private Mock<ISecurityLockoutService> _mockSecurityLockoutService;
    private ResetPasswordController _controller;
    private Mock<IDbTransaction> _mockTransaction;
    private Mock<HttpContext> _httpContextMock;

    [SetUp]
    public void Setup()
    {
        _mockConnection = new Mock<IDbConnection>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordResetRepository = new Mock<IPasswordResetRepository>();
        _mockSecurityLockoutService = new Mock<ISecurityLockoutService>();
        _mockTransaction = new Mock<IDbTransaction>();

        _mockConnection.Setup(c => c.BeginTransaction()).Returns(_mockTransaction.Object);
        _mockTransaction.Setup(t => t.Commit());
        _mockTransaction.Setup(t => t.Rollback());

        _controller = new ResetPasswordController(_mockConnection.Object, _mockUserRepository.Object, _mockPasswordResetRepository.Object, _mockSecurityLockoutService.Object);

        _httpContextMock = new Mock<HttpContext>();
        Mock<ConnectionInfo> connection = new();
        connection.Setup(c => c.RemoteIpAddress).Returns(new IPAddress(new byte[] { 127, 0, 0, 1 }));
        _httpContextMock.Setup(c => c.Connection).Returns(connection.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContextMock.Object
        };
    }

    [Test]
    public async Task ResetPassword_ValidRequest_ReturnsOkAndResetsAttempts()
    {
        ResetPasswordRequestModel requestModel = new()
        {
            Email = "test@example.com",
            Token = "valid_token",
            NewPassword = "NewSecurePassword123"
        };

        User user = new() { UserId = 1, Email = requestModel.Email, Name = "Test User", PasswordHash = "old_hash", Roles = new List<Role> { Role.Student } };
        _mockUserRepository.Setup(r => r.GetUserByEmailAsync(requestModel.Email, It.IsAny<IDbTransaction>())).ReturnsAsync(user);

        PasswordResetToken passwordResetToken = new()
        {
            TokenId = 1, // Dummy ID
            UserId = user.UserId,
            TokenHash = PasswordUtils.HashPasswordWithSalt(requestModel.Token),
            Expiration = DateTime.UtcNow.AddHours(1),
            IsUsed = false,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
        _mockPasswordResetRepository.Setup(r => r.GetPasswordResetTokenByEmail(requestModel.Email, It.IsAny<IDbTransaction>())).ReturnsAsync(passwordResetToken);

        // Act
        IActionResult result = await _controller.ResetPassword(requestModel);

        // Assert
        OkObjectResult? okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
            Assert.That(okResult.Value?.ToString(), Does.Contain("Password has been reset successfully."));
        }

        _mockSecurityLockoutService.Verify(s => s.ResetAttempts("IP:127.0.0.1", "Email:test@example.com"), Times.Once);
        _mockUserRepository.Verify(r => r.UpdateUserAsync(It.Is<User>(u => u.PasswordHash != "old_hash"), _mockTransaction.Object), Times.Once);
        _mockPasswordResetRepository.Verify(r => r.UpdatePasswordResetTokenAsync(It.Is<PasswordResetToken>(t => t.IsUsed), _mockTransaction.Object), Times.Once);
        _mockPasswordResetRepository.Verify(r => r.GetPasswordResetTokenByEmail(requestModel.Email, _mockTransaction.Object), Times.Once);
        _mockConnection.Verify(c => c.BeginTransaction(), Times.Once);
        _mockTransaction.Verify(t => t.Commit(), Times.Once);
        _mockTransaction.Verify(t => t.Rollback(), Times.Never);
    }

    [Test]
    public async Task ResetPassword_InvalidModelState_ReturnsBadRequest()
    {
        ResetPasswordRequestModel requestModel = new() { Email = "dummy@example.com", Token = "dummy_token", NewPassword = "DummyPassword123" };
        _controller.ModelState.AddModelError("Email", "Email is required");

        IActionResult result = await _controller.ResetPassword(requestModel);

        BadRequestObjectResult? badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
        }
        _mockTransaction.Verify(t => t.Commit(), Times.Never);
        _mockTransaction.Verify(t => t.Rollback(), Times.Never);
    }

    [Test]
    public async Task ResetPassword_UserNotFound_ReturnsBadRequest()
    {
        ResetPasswordRequestModel requestModel = new()
        {
            Email = "nonexistent@example.com",
            Token = "any_token",
            NewPassword = "NewSecurePassword123"
        };
        _mockUserRepository.Setup(r => r.GetUserByEmailAsync(requestModel.Email, It.IsAny<IDbTransaction>())).ReturnsAsync((User?)null);

        IActionResult result = await _controller.ResetPassword(requestModel);

        BadRequestObjectResult? badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
            Assert.That(badRequestResult.Value?.ToString(), Does.Contain("Failed to reset password."));
        }

        _mockConnection.Verify(c => c.BeginTransaction(), Times.Once);
        _mockTransaction.Verify(t => t.Rollback(), Times.Never);
        _mockTransaction.Verify(t => t.Commit(), Times.Never);
    }

    [Test]
    public async Task ResetPassword_PasswordResetTokenNotFound_ReturnsBadRequest()
    {
        ResetPasswordRequestModel requestModel = new()
        {
            Email = "test@example.com",
            Token = "valid_token",
            NewPassword = "NewSecurePassword123"
        };

        User user = new() { UserId = 1, Email = requestModel.Email, Name = "Test User", PasswordHash = "old_hash", Roles = new List<Role> { Role.Student } };
        _mockUserRepository.Setup(r => r.GetUserByEmailAsync(requestModel.Email, It.IsAny<IDbTransaction>())).ReturnsAsync(user);
        _mockPasswordResetRepository.Setup(r => r.GetPasswordResetTokenByEmail(requestModel.Email, It.IsAny<IDbTransaction>())).ReturnsAsync((PasswordResetToken?)null);

        IActionResult result = await _controller.ResetPassword(requestModel);

        BadRequestObjectResult? badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
            Assert.That(badRequestResult.Value?.ToString(), Does.Contain("Failed to reset password."));
        }

        _mockConnection.Verify(c => c.BeginTransaction(), Times.Once);
        _mockTransaction.Verify(t => t.Rollback(), Times.Once);
        _mockTransaction.Verify(t => t.Commit(), Times.Never);
    }
}
