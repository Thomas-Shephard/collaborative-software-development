using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Controllers.Auth;
using Jahoot.WebApi.Repositories;
using Jahoot.WebApi.Services;
using Jahoot.WebApi.Services.Background;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Auth;

public class ForgotPasswordControllerTests
{
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IPasswordResetRepository> _passwordResetRepositoryMock;
    private Mock<IEmailQueue> _emailQueueMock;
    private ForgotPasswordController _forgotPasswordController;

    [SetUp]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordResetRepositoryMock = new Mock<IPasswordResetRepository>();
        _emailQueueMock = new Mock<IEmailQueue>();
        _forgotPasswordController = new ForgotPasswordController(_userRepositoryMock.Object, _passwordResetRepositoryMock.Object, _emailQueueMock.Object);
    }

    [Test]
    public async Task ForgotPassword_InvalidModelState_ReturnsBadRequest()
    {
        _forgotPasswordController.ModelState.AddModelError("Email", "Required");
        ForgotPasswordRequestModel requestModel = new() { Email = "" };

        IActionResult result = await _forgotPasswordController.ForgotPassword(requestModel);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task ForgotPassword_UserNotFound_ReturnsOk()
    {
        ForgotPasswordRequestModel requestModel = new() { Email = "nonexistent@example.com" };
        _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(requestModel.Email)).ReturnsAsync((User?)null);

        IActionResult result = await _forgotPasswordController.ForgotPassword(requestModel);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        OkObjectResult okResult = (OkObjectResult)result;
        Assert.That(okResult.Value?.ToString(), Does.Contain("a password reset token has been sent."));

        _passwordResetRepositoryMock.Verify(passwordResetRepository => passwordResetRepository.CreateTokenAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        _emailQueueMock.Verify(emailService => emailService.QueueBackgroundEmailAsync(It.IsAny<EmailMessage>()), Times.Never);
    }

    [Test]
    public async Task ForgotPassword_UserFound_CreatesTokenAndSendsEmailAndReturnsOk()
    {
        ForgotPasswordRequestModel requestModel = new() { Email = "test@example.com" };
        User user = new() { UserId = 1, Email = requestModel.Email, Name = "User", PasswordHash = "hash", Roles = [] };
        _userRepositoryMock.Setup(userRepository => userRepository.GetUserByEmailAsync(requestModel.Email)).ReturnsAsync(user);

        string? capturedTokenHash = null;
        _passwordResetRepositoryMock.Setup(passwordResetRepository => passwordResetRepository.CreateTokenAsync(user.UserId, It.IsAny<string>()))
            .Callback<int, string>((_, tokenHash) => capturedTokenHash = tokenHash)
            .Returns(Task.CompletedTask);

        EmailMessage? capturedEmailMessage = null;
        _emailQueueMock.Setup(emailService => emailService.QueueBackgroundEmailAsync(It.IsAny<EmailMessage>()))
            .Callback<EmailMessage>(message => capturedEmailMessage = message)
            .Returns(ValueTask.CompletedTask);

        IActionResult result = await _forgotPasswordController.ForgotPassword(requestModel);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        OkObjectResult okResult = (OkObjectResult)result;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(okResult.Value?.ToString(), Does.Contain("a password reset token has been sent."));
            Assert.That(capturedTokenHash, Is.Not.Null);
            Assert.That(capturedEmailMessage, Is.Not.Null);
            Assert.That(capturedEmailMessage!.To, Is.EqualTo(user.Email));
            Assert.That(capturedEmailMessage.Subject, Is.EqualTo("Reset Your Jahoot Password"));
        }

        // Extract token from email body "Use the code {token} to reset your password. You have 15 minutes until the code expires."
        // Note: The controller format is "Use the code {token} to reset your password..."
        string plainToken = capturedEmailMessage!.Body.Split("Use the code ")[1].Split(" to reset your password")[0];

        //Assert.That(PasswordUtils.VerifyPassword(plainToken, capturedTokenHash), Is.True);

        _passwordResetRepositoryMock.Verify(passwordResetRepository => passwordResetRepository.CreateTokenAsync(user.UserId, capturedTokenHash), Times.Once);
        _emailQueueMock.Verify(s => s.QueueBackgroundEmailAsync(It.Is<EmailMessage>(m => m.To == user.Email && m.Body.Contains(plainToken))), Times.Once);
    }
}
