using Moq;
using System.Data;
using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.Core.Utils;
using Jahoot.WebApi.Repositories;
using Jahoot.WebApi.Controllers.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Tests.Controllers.Auth;

public class ResetPasswordControllerTests
{
    private Mock<IDbConnection> _mockConnection;
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IPasswordResetRepository> _mockPasswordResetRepository;
    private ResetPasswordController _controller;
    private Mock<IDbTransaction> _mockTransaction;

    [SetUp]
    public void Setup()
    {
        _mockConnection = new Mock<IDbConnection>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordResetRepository = new Mock<IPasswordResetRepository>();
        _mockTransaction = new Mock<IDbTransaction>();

        _mockConnection.Setup(c => c.BeginTransaction()).Returns(_mockTransaction.Object);
        _mockTransaction.Setup(t => t.Commit());
        _mockTransaction.Setup(t => t.Rollback());

        _controller = new ResetPasswordController(_mockConnection.Object, _mockUserRepository.Object, _mockPasswordResetRepository.Object);
    }

    [Test]
    public async Task ResetPassword_ValidRequest_ReturnsOk()
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
            Assert.That(badRequestResult.Value, Is.InstanceOf<SerializableError>());
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
        _mockTransaction.Verify(t => t.Rollback(), Times.Never);
        _mockTransaction.Verify(t => t.Commit(), Times.Never);
    }

    [Test]
    public async Task ResetPassword_PasswordResetTokenIsUsed_ReturnsBadRequest()
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
            IsUsed = true, // Token is used
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
        _mockPasswordResetRepository.Setup(r => r.GetPasswordResetTokenByEmail(requestModel.Email, It.IsAny<IDbTransaction>())).ReturnsAsync(passwordResetToken);

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
    public async Task ResetPassword_PasswordResetTokenIsRevoked_ReturnsBadRequest()
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
            IsRevoked = true, // Token is revoked
            CreatedAt = DateTime.UtcNow
        };
        _mockPasswordResetRepository.Setup(r => r.GetPasswordResetTokenByEmail(requestModel.Email, It.IsAny<IDbTransaction>())).ReturnsAsync(passwordResetToken);

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
    public async Task ResetPassword_PasswordResetTokenIsExpired_ReturnsBadRequest()
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
            Expiration = DateTime.UtcNow.AddHours(-1), // Token is expired
            IsUsed = false,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
        _mockPasswordResetRepository.Setup(r => r.GetPasswordResetTokenByEmail(requestModel.Email, It.IsAny<IDbTransaction>())).ReturnsAsync(passwordResetToken);

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
    public async Task ResetPassword_IncorrectTokenHash_ReturnsBadRequest()
    {
        ResetPasswordRequestModel requestModel = new()
        {
            Email = "test@example.com",
            Token = "inco__", // This token won't match the hashed one
            NewPassword = "NewSecurePassword123"
        };

        User user = new() { UserId = 1, Email = requestModel.Email, Name = "Test User", PasswordHash = "old_hash", Roles = new List<Role> { Role.Student } };
        _mockUserRepository.Setup(r => r.GetUserByEmailAsync(requestModel.Email, It.IsAny<IDbTransaction>())).ReturnsAsync(user);

        PasswordResetToken passwordResetToken = new()
        {
            TokenId = 1, // Dummy ID
            UserId = user.UserId,
            TokenHash = PasswordUtils.HashPasswordWithSalt("correct_token"), // Stored hash is for "correct_token"
            Expiration = DateTime.UtcNow.AddHours(1),
            IsUsed = false,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
        _mockPasswordResetRepository.Setup(r => r.GetPasswordResetTokenByEmail(requestModel.Email, It.IsAny<IDbTransaction>())).ReturnsAsync(passwordResetToken);

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
    public void ResetPassword_ExceptionDuringTransaction_RollsBackAndThrows()
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

        _mockUserRepository.Setup(r => r.UpdateUserAsync(It.IsAny<User>(), It.IsAny<IDbTransaction>())).ThrowsAsync(new InvalidOperationException("Simulated DB error"));

        Assert.That(async () => await _controller.ResetPassword(requestModel), Throws.TypeOf<InvalidOperationException>().And.Message.EqualTo("Simulated DB error"));

        _mockConnection.Verify(c => c.BeginTransaction(), Times.Once);
        _mockTransaction.Verify(t => t.Rollback(), Times.Once);
        _mockTransaction.Verify(t => t.Commit(), Times.Never);
    }

    [Test]
    public async Task ResetPassword_ConnectionClosed_OpensConnectionAndSucceeds()
    {
        ResetPasswordRequestModel requestModel = new()
        {
            Email = "test@example.com",
            Token = "valid_token",
            NewPassword = "NewSecurePassword123"
        };

        // Ensure the mock connection starts as closed
        _mockConnection.SetupGet(c => c.State).Returns(ConnectionState.Closed);
        _mockConnection.Setup(c => c.Open()).Callback(() => _mockConnection.SetupGet(c => c.State).Returns(ConnectionState.Open));

        User user = new() { UserId = 1, Email = requestModel.Email, Name = "Test User", PasswordHash = "old_hash", Roles = new List<Role> { Role.Student } };
        _mockUserRepository.Setup(r => r.GetUserByEmailAsync(requestModel.Email, It.IsAny<IDbTransaction>())).ReturnsAsync(user);

        PasswordResetToken passwordResetToken = new()
        {
            TokenId = 1,
            UserId = user.UserId,
            TokenHash = PasswordUtils.HashPasswordWithSalt(requestModel.Token),
            Expiration = DateTime.UtcNow.AddHours(1),
            IsUsed = false,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
        _mockPasswordResetRepository.Setup(r => r.GetPasswordResetTokenByEmail(requestModel.Email, It.IsAny<IDbTransaction>())).ReturnsAsync(passwordResetToken);

        _mockUserRepository.Setup(r => r.UpdateUserAsync(It.IsAny<User>(), It.IsAny<IDbTransaction>())).Returns(Task.CompletedTask);
        _mockPasswordResetRepository.Setup(r => r.UpdatePasswordResetTokenAsync(It.IsAny<PasswordResetToken>(), It.IsAny<IDbTransaction>())).Returns(Task.CompletedTask);

        IActionResult result = await _controller.ResetPassword(requestModel);

        OkObjectResult? okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
            Assert.That(okResult.Value?.ToString(), Does.Contain("Password has been reset successfully."));
        }

        _mockConnection.Verify(c => c.Open(), Times.Once());
        _mockConnection.Verify(c => c.BeginTransaction(), Times.Once());
        _mockTransaction.Verify(t => t.Commit(), Times.Once());
        _mockTransaction.Verify(t => t.Rollback(), Times.Never());
    }
}
