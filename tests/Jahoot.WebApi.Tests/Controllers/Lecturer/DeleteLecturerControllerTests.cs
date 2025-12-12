using System.Reflection;
using Jahoot.Core.Models;
using Jahoot.WebApi.Controllers.Lecturer;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Lecturer;

public class DeleteLecturerControllerTests
{
    private Mock<ILecturerRepository> _lecturerRepositoryMock;
    private Mock<IUserRepository> _userRepositoryMock;
    private DeleteLecturerController _controller;

    [SetUp]
    public void Setup()
    {
        _lecturerRepositoryMock = new Mock<ILecturerRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _controller = new DeleteLecturerController(_lecturerRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Test]
    public async Task DeleteLecturer_LecturerNotFound_ReturnsNotFound()
    {
        const int userId = 1;
        _lecturerRepositoryMock.Setup(repo => repo.GetLecturerByUserIdAsync(userId)).ReturnsAsync((Core.Models.Lecturer?)null);

        IActionResult result = await _controller.DeleteLecturer(userId);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult notFoundResult = (NotFoundObjectResult)result;
        Assert.That(notFoundResult.Value, Is.EqualTo($"Lecturer with user ID {userId} not found."));
    }

    [Test]
    public async Task DeleteLecturer_LecturerExists_DeletesUserAndReturnsOk()
    {
        const int userId = 1;
        Core.Models.Lecturer lecturer = new() { LecturerId = 1, UserId = userId, Email = "test@example.com", Name = "Test", PasswordHash = "hash", Roles = [] };
        _lecturerRepositoryMock.Setup(repo => repo.GetLecturerByUserIdAsync(userId)).ReturnsAsync(lecturer);

        IActionResult result = await _controller.DeleteLecturer(userId);

        Assert.That(result, Is.TypeOf<OkResult>());
        _userRepositoryMock.Verify(repo => repo.DeleteUserAsync(userId), Times.Once);
    }

    [Test]
    public void DeleteLecturer_HasAdminAuthorization()
    {
        MethodInfo? methodInfo = typeof(DeleteLecturerController).GetMethod(nameof(DeleteLecturerController.DeleteLecturer));
        object[]? attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        Assert.That(attributes, Is.Not.Null);
        AuthorizeAttribute? authorizeAttribute = attributes.OfType<AuthorizeAttribute>().FirstOrDefault(a => a.Policy == nameof(Role.Admin));
        Assert.That(authorizeAttribute, Is.Not.Null, "Method should have [Authorize(Policy = \"Admin\")]");
    }
}
