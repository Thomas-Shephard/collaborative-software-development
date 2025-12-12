using System.Reflection;
using Jahoot.Core.Models;
using Jahoot.WebApi.Controllers.Student;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Jahoot.WebApi.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using StudentModel = Jahoot.Core.Models.Student;

namespace Jahoot.WebApi.Tests.Controllers.Student;

public class ListStudentsControllerTests
{
    private Mock<IStudentRepository> _studentRepositoryMock;
    private ListStudentsController _controller;

    [SetUp]
    public void Setup()
    {
        _studentRepositoryMock = new Mock<IStudentRepository>();
        _controller = new ListStudentsController(_studentRepositoryMock.Object);
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
    public async Task GetStudents_WithLecturerRole_ReturnsOkWithStudents()
    {
        SetupUserClaims(Role.Lecturer);
        const StudentAccountStatus status = StudentAccountStatus.PendingApproval;

        List<StudentModel> students =
        [
            new()
            {
                UserId = 1,
                Email = "pending1@test.com",
                Name = "Pending Student 1",
                PasswordHash = "hash1",
                Roles = new List<Role> { Role.Student },
                StudentId = 101,
                AccountStatus = StudentAccountStatus.PendingApproval
            },

            new()
            {
                UserId = 2,
                Email = "pending2@test.com",
                Name = "Pending Student 2",
                PasswordHash = "hash2",
                Roles = new List<Role> { Role.Student },
                StudentId = 102,
                AccountStatus = StudentAccountStatus.PendingApproval
            }
        ];

        _studentRepositoryMock.Setup(repo => repo.GetStudentsByStatusAsync(status))
                              .ReturnsAsync(students);

        IActionResult result = await _controller.GetStudents(status);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        OkObjectResult? okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        // Ensure that PasswordHash is not present in the returned DTOs
        StudentResponseDto[]? studentDtos = (okResult.Value as IEnumerable<StudentResponseDto>)?.ToArray();
        Assert.That(studentDtos, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(studentDtos, Has.Length.EqualTo(2));
            Assert.That(studentDtos.All(dto => string.IsNullOrEmpty(dto.GetType().GetProperty("PasswordHash")?.GetValue(dto)?.ToString())), Is.True, "PasswordHash should not be present in DTOs");
        }

        _studentRepositoryMock.Verify(repo => repo.GetStudentsByStatusAsync(status), Times.Once);
    }

    [Test]
    public async Task GetStudents_InvalidStatusEnumValue_ReturnsBadRequest()
    {
        SetupUserClaims(Role.Lecturer);
        const StudentAccountStatus invalidStatus = (StudentAccountStatus)999; // Cast an invalid integer to the enum

        IActionResult result = await _controller.GetStudents(invalidStatus);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult? badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult?.Value?.ToString(), Does.Contain("Invalid student account status"));
    }

    [Test]
    public void GetStudents_RequiresLecturerAuthorization()
    {
        MethodInfo? methodInfo = typeof(ListStudentsController).GetMethod(nameof(ListStudentsController.GetStudents));
        object[]? attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        Assert.That(attributes, Is.Not.Null);

        AuthorizeAttribute? authorizeAttribute = attributes.OfType<AuthorizeAttribute>().FirstOrDefault(a => a.Policy == nameof(Role.Lecturer));
        Assert.That(authorizeAttribute, Is.Not.Null, "Method should have [Authorize(Policy = \"Lecturer\")]");
    }
}
