using System.Reflection;
using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Controllers.Lecturer;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Lecturer;

public class UpdateLecturerControllerTests
{
    private Mock<ILecturerRepository> _lecturerRepositoryMock;
    private Mock<IUserRepository> _userRepositoryMock;
    private UpdateLecturerController _controller;

    [SetUp]
    public void Setup()
    {
        _lecturerRepositoryMock = new Mock<ILecturerRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _controller = new UpdateLecturerController(_lecturerRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Test]
    public async Task UpdateLecturer_LecturerNotFound_ReturnsNotFound()
    {
        const int userId = 1;
        UpdateLecturerRequestModel request = new() { Email = "test@example.com", Name = "Test", IsAdmin = false, IsDisabled = false };
        _lecturerRepositoryMock.Setup(repo => repo.GetLecturerByUserIdAsync(userId)).ReturnsAsync((Core.Models.Lecturer?)null);

        IActionResult result = await _controller.UpdateLecturer(userId, request);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult notFoundResult = (NotFoundObjectResult)result;
        Assert.That(notFoundResult.Value, Is.EqualTo($"Lecturer with user ID {userId} not found."));
    }

    [Test]
    public async Task UpdateLecturer_EmailConflict_ReturnsConflict()
    {
        const int userId = 1;
        UpdateLecturerRequestModel request = new() { Email = "other@example.com", Name = "Test", IsAdmin = false, IsDisabled = false };
        Core.Models.Lecturer lecturer = new() { LecturerId = 1, UserId = userId, Email = "test@example.com", Name = "Test", PasswordHash = "hash", Roles = [] };
        User otherUser = new() { UserId = 2, Email = request.Email, Name = "Other", PasswordHash = "hash", Roles = [] };

        _lecturerRepositoryMock.Setup(repo => repo.GetLecturerByUserIdAsync(userId)).ReturnsAsync(lecturer);
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(request.Email)).ReturnsAsync(otherUser);

        IActionResult result = await _controller.UpdateLecturer(userId, request);

        Assert.That(result, Is.TypeOf<ConflictObjectResult>());
        ConflictObjectResult conflictResult = (ConflictObjectResult)result;
        Assert.That(conflictResult.Value, Is.EqualTo("A user with this email address already exists."));
    }

    [Test]
    public async Task UpdateLecturer_ValidRequest_ReturnsOkAndUpdates()
    {
        const int userId = 1;
        UpdateLecturerRequestModel request = new() { Email = "new@example.com", Name = "New Name", IsAdmin = true, IsDisabled = true };
        Core.Models.Lecturer lecturer = new() { LecturerId = 1, UserId = userId, Email = "old@example.com", Name = "Old Name", IsAdmin = false, PasswordHash = "hash", Roles = [] };

        _lecturerRepositoryMock.Setup(repo => repo.GetLecturerByUserIdAsync(userId)).ReturnsAsync(lecturer);
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(request.Email)).ReturnsAsync((User?)null);

        IActionResult result = await _controller.UpdateLecturer(userId, request);

        Assert.That(result, Is.TypeOf<OkResult>());
        _userRepositoryMock.Verify(repo => repo.UpdateUserAsync(It.Is<Core.Models.Lecturer>(l => l.Email == request.Email && l.Name == request.Name && l.IsDisabled == request.IsDisabled)), Times.Once);
        _lecturerRepositoryMock.Verify(repo => repo.UpdateLecturerAsync(It.Is<Core.Models.Lecturer>(l => l.IsAdmin == request.IsAdmin)), Times.Once);
    }

    [Test]
    public async Task UpdateLecturer_InvalidModel_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("Email", "Invalid email");
        UpdateLecturerRequestModel request = new() { Email = "bad", Name = "Name", IsAdmin = false, IsDisabled = false };

        IActionResult result = await _controller.UpdateLecturer(1, request);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        _lecturerRepositoryMock.Verify(repo => repo.UpdateLecturerAsync(It.IsAny<Core.Models.Lecturer>()), Times.Never);
    }

    [Test]
    public void UpdateLecturer_HasAdminAuthorization()
    {
        MethodInfo? methodInfo = typeof(UpdateLecturerController).GetMethod(nameof(UpdateLecturerController.UpdateLecturer));
        object[]? attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        Assert.That(attributes, Is.Not.Null);
        AuthorizeAttribute? authorizeAttribute = attributes.OfType<AuthorizeAttribute>().FirstOrDefault(a => a.Policy == nameof(Role.Admin));
        Assert.That(authorizeAttribute, Is.Not.Null, "Method should have [Authorize(Policy = \"Admin\")]");
    }
}
