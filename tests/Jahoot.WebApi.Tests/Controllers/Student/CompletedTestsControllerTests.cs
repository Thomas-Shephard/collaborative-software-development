using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Jahoot.Core.Models;
using Jahoot.WebApi.Controllers.Student;
using Jahoot.WebApi.Models.Responses;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Student;

public class CompletedTestsControllerTests
{
    private Mock<IStudentRepository> _studentRepositoryMock;
    private Mock<ITestRepository> _testRepositoryMock;
    private CompletedTestsController _controller;

    [SetUp]
    public void Setup()
    {
        _studentRepositoryMock = new Mock<IStudentRepository>();
        _testRepositoryMock = new Mock<ITestRepository>();
        _controller = new CompletedTestsController(_studentRepositoryMock.Object, _testRepositoryMock.Object);
    }

    private void SetupUser(object? userId)
    {
        Claim[] claims = [new(ClaimTypes.NameIdentifier, userId?.ToString() ?? "")];
        ClaimsPrincipal user = new(new ClaimsIdentity(claims, "TestAuth"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Test]
    public async Task GetCompletedTests_ReturnsOkWithTests()
    {
        const int userId = 1;
        const int studentId = 10;
        SetupUser(userId);

        Core.Models.Student student = new()
        {
            UserId = userId,
            StudentId = studentId,
            Email = "student@test.com",
            Name = "Test Student",
            IsApproved = true,
            Roles = [Role.Student],
            Subjects = []
        };

        List<CompletedTestResponse> completedTests =
        [
            new() { TestName = "Test 1", SubjectName = "Subject 1" },
            new() { TestName = "Test 2", SubjectName = "Subject 2" }
        ];

        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync(student);
        _testRepositoryMock.Setup(repo => repo.GetCompletedTestsForStudentAsync(studentId)).ReturnsAsync(completedTests);

        ActionResult<IEnumerable<CompletedTestResponse>> result = await _controller.GetCompletedTests();

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(completedTests));
    }

    [Test]
    public async Task GetCompletedTests_WhenStudentIdInvalid_ReturnsBadRequest()
    {
        const string userId = "invalid";
        SetupUser(userId);

        ActionResult<IEnumerable<CompletedTestResponse>> result = await _controller.GetCompletedTests();

        Assert.That(result.Result, Is.TypeOf<BadRequestResult>());
    }

    [Test]
    public async Task GetCompletedTests_WhenStudentNotFound_ReturnsNotFound()
    {
        const int userId = 1;
        SetupUser(userId);
        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync((Core.Models.Student?)null);

        ActionResult<IEnumerable<CompletedTestResponse>> result = await _controller.GetCompletedTests();

        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }
}
