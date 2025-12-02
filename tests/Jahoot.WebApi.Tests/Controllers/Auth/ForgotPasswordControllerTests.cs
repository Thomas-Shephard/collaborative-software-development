using Jahoot.Core.Models;
using Jahoot.WebApi.Controllers.Auth;
using Jahoot.WebApi.Repositories;
using Jahoot.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Auth;

public class ForgotPasswordControllerTests
{
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IPasswordResetRepository> _passwordResetRepositoryMock;
    private Mock<IEmailService> _emailServiceMock;
    private ForgotPasswordController _forgotPasswordController;

    [SetUp]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordResetRepositoryMock = new Mock<IPasswordResetRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _forgotPasswordController = new ForgotPasswordController(_userRepositoryMock.Object, _passwordResetRepositoryMock.Object, _emailServiceMock.Object);
    }

    [Test]
    public async Task ForgotPassword_InvalidModelState_ReturnsBadRequest()
    {
        _forgotPasswordController.ModelState.AddModelError("Email", "Required");
        ForgotPasswordRequest request = new() { Email = "" };

        IActionResult result = await _forgotPasswordController.ForgotPassword(request);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task ForgotPassword_UserNotFound_ReturnsOk()
    {
        ForgotPasswordRequest request = new() { Email = "nonexistent@example.com" };
        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(request.Email)).ReturnsAsync((User?)null);

        IActionResult result = await _forgotPasswordController.ForgotPassword(request);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        OkObjectResult okResult = (OkObjectResult)result;
        Assert.That(okResult.Value?.ToString(), Does.Contain("a password reset token has been sent."));

        _passwordResetRepositoryMock.Verify(passwordResetRepository => passwordResetRepository.CreateTokenAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        _emailServiceMock.Verify(emailService => emailService.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task ForgotPassword_UserFound_CreatesTokenAndSendsEmailAndReturnsOk()
    {
        ForgotPasswordRequest request = new() { Email = "test@example.com" };
        User user = new() { UserId = 1, Email = request.Email, Name = "User", PasswordHash = "hash", Roles = [] };
        _userRepositoryMock.Setup(userRepository => userRepository.GetUserByEmailAsync(request.Email)).ReturnsAsync(user);

        string? capturedToken = null;
        _passwordResetRepositoryMock.Setup(passwordResetRepository => passwordResetRepository.CreateTokenAsync(user.UserId, It.IsAny<string>()))
            .Callback<int, string>((_, token) => capturedToken = token)
            .Returns(Task.CompletedTask);

        IActionResult result = await _forgotPasswordController.ForgotPassword(request);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        OkObjectResult okResult = (OkObjectResult)result;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(okResult.Value?.ToString(), Does.Contain("a password reset token has been sent."));
            Assert.That(capturedToken, Is.Not.Null);
        }
        _passwordResetRepositoryMock.Verify(passwordResetRepository => passwordResetRepository.CreateTokenAsync(user.UserId, capturedToken), Times.Once);
        _emailServiceMock.Verify(s => s.SendEmailAsync(user.Email, "Jahoot Password Reset", It.Is<string>(body => body.Contains(capturedToken))), Times.Once);
    }
}
