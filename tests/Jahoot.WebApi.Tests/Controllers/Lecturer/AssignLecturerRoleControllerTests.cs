using System.Reflection;
using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Controllers.Lecturer;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Lecturer;

public class AssignLecturerRoleControllerTests
{
    private Mock<ILecturerRepository> _lecturerRepositoryMock;
    private Mock<IUserRepository> _userRepositoryMock;
    private AssignLecturerRoleController _controller;

    [SetUp]
    public void Setup()
    {
        _lecturerRepositoryMock = new Mock<ILecturerRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _controller = new AssignLecturerRoleController(_lecturerRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Test]
    public async Task AssignRoleToExistingUser_UserNotFound_ReturnsNotFound()
    {
        AssignLecturerRoleRequestModel request = new() { Email = "nonexistent@example.com", IsAdmin = false };
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(request.Email)).ReturnsAsync((User?)null);

        IActionResult result = await _controller.AssignRoleToExistingUser(request);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult notFoundResult = (NotFoundObjectResult)result;
        Assert.That(notFoundResult.Value, Is.EqualTo("User with provided email not found."));
    }

    [Test]
    public async Task AssignRoleToExistingUser_AlreadyLecturer_ReturnsConflict()
    {
        AssignLecturerRoleRequestModel request = new() { Email = "lecturer@example.com", IsAdmin = false };
        User user = new() { UserId = 1, Email = request.Email, Name = "Lecturer", PasswordHash = "hash", Roles = [] };
        Core.Models.Lecturer lecturer = new() { LecturerId = 1, UserId = 1, IsAdmin = false, Email = request.Email, Name = "Lecturer", PasswordHash = "hash", Roles = [] };

        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(request.Email)).ReturnsAsync(user);
        _lecturerRepositoryMock.Setup(repo => repo.GetLecturerByUserIdAsync(user.UserId)).ReturnsAsync(lecturer);

        IActionResult result = await _controller.AssignRoleToExistingUser(request);

        Assert.That(result, Is.TypeOf<ConflictObjectResult>());
        ConflictObjectResult conflictResult = (ConflictObjectResult)result;
        Assert.That(conflictResult.Value, Is.EqualTo("User is already assigned the lecturer role."));
    }

    [Test]
    public async Task AssignRoleToExistingUser_ValidRequest_ReturnsOk()
    {
        AssignLecturerRoleRequestModel request = new() { Email = "user@example.com", IsAdmin = true };
        User user = new() { UserId = 1, Email = request.Email, Name = "User", PasswordHash = "hash", Roles = [] };

        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(request.Email)).ReturnsAsync(user);
        _lecturerRepositoryMock.Setup(repo => repo.GetLecturerByUserIdAsync(user.UserId)).ReturnsAsync((Core.Models.Lecturer?)null);

        IActionResult result = await _controller.AssignRoleToExistingUser(request);

        Assert.That(result, Is.TypeOf<OkResult>());
        _lecturerRepositoryMock.Verify(repo => repo.CreateLecturerAsync(user.UserId, request.IsAdmin), Times.Once);
    }

    [Test]
    public async Task AssignRoleToExistingUser_InvalidModel_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("Email", "Invalid email");
        AssignLecturerRoleRequestModel request = new() { Email = "bad", IsAdmin = false };

        IActionResult result = await _controller.AssignRoleToExistingUser(request);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        _lecturerRepositoryMock.Verify(repo => repo.CreateLecturerAsync(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void AssignRoleToExistingUser_HasAdminAuthorization()
    {
        MethodInfo? methodInfo = typeof(AssignLecturerRoleController).GetMethod(nameof(AssignLecturerRoleController.AssignRoleToExistingUser));
        object[]? attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        Assert.That(attributes, Is.Not.Null);
        AuthorizeAttribute? authorizeAttribute = attributes.OfType<AuthorizeAttribute>().FirstOrDefault(a => a.Policy == nameof(Role.Admin));
        Assert.That(authorizeAttribute, Is.Not.Null, "Method should have [Authorize(Policy = \"Admin\")]");
    }
}
