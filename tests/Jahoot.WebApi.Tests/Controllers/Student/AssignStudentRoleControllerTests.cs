using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.Core.Utils;
using Jahoot.WebApi.Controllers.Student;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Student;

public class AssignStudentRoleControllerTests
{
    private const string UserEmail = "test@example.com";
    private const string UserPassword = "password123";

    private Mock<IStudentRepository> _studentRepositoryMock;
    private Mock<IUserRepository> _userRepositoryMock;
    private AssignStudentRoleController _controller;

    [SetUp]
    public void Setup()
    {
        _studentRepositoryMock = new Mock<IStudentRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _controller = new AssignStudentRoleController(_studentRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Test]
    public async Task AssignRoleToExistingUser_UserNotFound_ReturnsUnauthorized()
    {
        AssignStudentRoleRequestModel request = new() { Email = "nonexistent@example.com", Password = "password" };
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(request.Email)).ReturnsAsync((User?)null);

        IActionResult result = await _controller.RegisterExistingStudent(request);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
            Assert.That(((UnauthorizedObjectResult)result).Value, Is.EqualTo("Invalid email or password."));
        }
    }

    [Test]
    public async Task AssignRoleToExistingUser_WrongPassword_ReturnsUnauthorized()
    {
        AssignStudentRoleRequestModel request = new() { Email = UserEmail, Password = $"Incorrect-{UserPassword}" };
        User user = new()
        {
            UserId = 1,
            Email = UserEmail,
            Name = "Test User",
            PasswordHash = PasswordUtils.HashPasswordWithSalt(UserPassword),
            Roles = new List<Role> { Role.Lecturer }
        };
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(UserEmail)).ReturnsAsync(user);

        IActionResult result = await _controller.RegisterExistingStudent(request);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
            Assert.That(((UnauthorizedObjectResult)result).Value, Is.EqualTo("Invalid email or password."));
        }
    }

    [Test]
    public async Task AssignRoleToExistingUser_AlreadyStudent_ReturnsConflict()
    {
        AssignStudentRoleRequestModel request = new() { Email = UserEmail, Password = UserPassword };
        User user = new()
        {
            UserId = 1,
            Email = UserEmail,
            Name = "Student User",
            PasswordHash = PasswordUtils.HashPasswordWithSalt(UserPassword),
            Roles = new List<Role> { Role.Student }
        };
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(UserEmail)).ReturnsAsync(user);

        IActionResult result = await _controller.RegisterExistingStudent(request);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.TypeOf<ConflictObjectResult>());
            Assert.That(((ConflictObjectResult)result).Value, Is.EqualTo("User is already a student."));
        }
    }

    [Test]
    public async Task AssignRoleToExistingUser_ValidRequest_ReturnsOk()
    {
        AssignStudentRoleRequestModel request = new() { Email = UserEmail, Password = UserPassword };
        User user = new()
        {
            UserId = 1,
            Email = UserEmail,
            Name = "Lecturer User",
            PasswordHash = PasswordUtils.HashPasswordWithSalt(UserPassword),
            Roles = new List<Role> { Role.Lecturer }
        };
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(UserEmail)).ReturnsAsync(user);

        IActionResult result = await _controller.RegisterExistingStudent(request);

        Assert.That(result, Is.TypeOf<OkResult>());
        _studentRepositoryMock.Verify(repo => repo.CreateStudentAsync(user.UserId), Times.Once);
    }

    [Test]
    public async Task AssignRoleToExistingUser_InvalidModel_ReturnsBadRequest()
    {
         _controller.ModelState.AddModelError("Email", "Invalid email");
         AssignStudentRoleRequestModel request = new() { Email = "bad", Password = "pass" };

         IActionResult result = await _controller.RegisterExistingStudent(request);

         Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
         _studentRepositoryMock.Verify(repo => repo.CreateStudentAsync(It.IsAny<int>()), Times.Never);
    }
}
