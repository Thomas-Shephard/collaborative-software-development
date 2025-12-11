using System.Reflection;
using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Controllers.Student;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using StudentModel = Jahoot.Core.Models.Student;

namespace Jahoot.WebApi.Tests.Controllers.Student;

public class UpdateStudentControllerTests
{
    private Mock<IStudentRepository> _studentRepositoryMock;
    private Mock<IUserRepository> _userRepositoryMock;
    private UpdateStudentController _controller;

    [SetUp]
    public void Setup()
    {
        _studentRepositoryMock = new Mock<IStudentRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _controller = new UpdateStudentController(_studentRepositoryMock.Object, _userRepositoryMock.Object);
    }

    private void SetupUserClaims(params Role[] roles)
    {
        Claim[] claims = roles.Select(role => new Claim(ClaimTypes.Role, role.ToString())).ToArray();
        ClaimsPrincipal user = new(new ClaimsIdentity(claims, "TestAuth"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Test]
    public async Task UpdateStudent_WithValidData_ReturnsOkAndUpdates()
    {
        SetupUserClaims(Role.Lecturer);
        const int userId = 1;
        UpdateStudentRequestModel requestModel = new()
        {
            Name = "New Name",
            Email = "new@test.com",
            AccountStatus = StudentAccountStatus.Active
        };

        StudentModel student = new()
        {
            UserId = userId,
            Email = "old@test.com",
            Name = "Old Name",
            PasswordHash = "hash",
            Roles = new List<Role> { Role.Student },
            StudentId = 101,
            AccountStatus = StudentAccountStatus.PendingApproval
        };

        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync(student);
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(requestModel.Email)).ReturnsAsync((User?)null);

        IActionResult result = await _controller.UpdateStudent(userId, requestModel);

        Assert.That(result, Is.TypeOf<OkResult>());

        _studentRepositoryMock.Verify(repo => repo.UpdateStudentAsync(It.Is<StudentModel>(s =>
            s.AccountStatus == requestModel.AccountStatus)), Times.Once);

        _userRepositoryMock.Verify(repo => repo.UpdateUserAsync(It.Is<StudentModel>(s =>
            s.Name == requestModel.Name &&
            s.Email == requestModel.Email)), Times.Once);
    }

    [Test]
    public async Task UpdateStudent_StudentNotFound_ReturnsNotFound()
    {
        SetupUserClaims(Role.Lecturer);
        const int userId = 1;
        UpdateStudentRequestModel requestModel = new()
        {
            Name = "New Name",
            Email = "new@test.com",
            AccountStatus = StudentAccountStatus.Active
        };

        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync((StudentModel?)null);

        IActionResult result = await _controller.UpdateStudent(userId, requestModel);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task UpdateStudent_EmailConflict_ReturnsConflict()
    {
        SetupUserClaims(Role.Lecturer);
        const int userId = 1;
        UpdateStudentRequestModel requestModel = new()
        {
            Name = "New Name",
            Email = "taken@test.com",
            AccountStatus = StudentAccountStatus.Active
        };

        StudentModel student = new()
        {
            UserId = userId,
            Email = "old@test.com",
            Name = "Old Name",
            PasswordHash = "hash",
            Roles = new List<Role> { Role.Student },
            StudentId = 101,
            AccountStatus = StudentAccountStatus.PendingApproval
        };

        User otherUser = new()
        {
            UserId = 2, // Different user
            Email = "taken@test.com",
            Name = "Other User",
            PasswordHash = "hash",
            Roles = new List<Role> { Role.Student },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync(student);
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(requestModel.Email)).ReturnsAsync(otherUser);

        IActionResult result = await _controller.UpdateStudent(userId, requestModel);

        Assert.That(result, Is.TypeOf<ConflictObjectResult>());
    }

    [Test]
    public void UpdateStudent_RequiresLecturerAuthorization()
    {
        MethodInfo? methodInfo = typeof(UpdateStudentController).GetMethod(nameof(UpdateStudentController.UpdateStudent));
        object[]? attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        Assert.That(attributes, Is.Not.Null);

        AuthorizeAttribute? authorizeAttribute = attributes.OfType<AuthorizeAttribute>().FirstOrDefault(a => a.Policy == nameof(Role.Lecturer));
        Assert.That(authorizeAttribute, Is.Not.Null, "Method should have [Authorize(Policy = \"Lecturer\")]");
    }
}
