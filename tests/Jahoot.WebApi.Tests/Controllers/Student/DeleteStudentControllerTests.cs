using System.Reflection;
using Jahoot.Core.Models;
using Jahoot.WebApi.Controllers.Student;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Student;

public class DeleteStudentControllerTests
{
    private Mock<IStudentRepository> _studentRepositoryMock;
    private Mock<IUserRepository> _userRepositoryMock;
    private DeleteStudentController _controller;

    [SetUp]
    public void Setup()
    {
        _studentRepositoryMock = new Mock<IStudentRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _controller = new DeleteStudentController(_studentRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Test]
    public async Task DeleteStudent_StudentNotFound_ReturnsNotFound()
    {
        const int userId = 1;
        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync((Core.Models.Student?)null);

        IActionResult result = await _controller.DeleteStudent(userId);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult notFoundResult = (NotFoundObjectResult)result;
        Assert.That(notFoundResult.Value, Is.EqualTo($"Student with user ID {userId} not found."));
    }

    [Test]
    public async Task DeleteStudent_StudentExists_DeletesUserAndReturnsOk()
    {
        const int userId = 1;
        Core.Models.Student student = new() { StudentId = 1, UserId = userId, Email = "test@example.com", Name = "Test", PasswordHash = "hash", Roles = [], AccountStatus = StudentAccountStatus.Active };
        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync(student);

        IActionResult result = await _controller.DeleteStudent(userId);

        Assert.That(result, Is.TypeOf<OkResult>());
        _userRepositoryMock.Verify(repo => repo.DeleteUserAsync(userId), Times.Once);
    }

    [Test]
    public void DeleteStudent_HasAdminAuthorization()
    {
        MethodInfo? methodInfo = typeof(DeleteStudentController).GetMethod(nameof(DeleteStudentController.DeleteStudent));
        object[]? attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        Assert.That(attributes, Is.Not.Null);
        AuthorizeAttribute? authorizeAttribute = attributes.OfType<AuthorizeAttribute>().FirstOrDefault(a => a.Policy == nameof(Role.Lecturer));
        Assert.That(authorizeAttribute, Is.Not.Null, "Method should have [Authorize(Policy = \"Lecturer\")]");
    }
}
