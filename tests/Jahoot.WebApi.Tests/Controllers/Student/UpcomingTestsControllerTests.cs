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

public class UpcomingTestsControllerTests
{
    private Mock<IStudentRepository> _studentRepositoryMock;
    private Mock<ITestRepository> _testRepositoryMock;
    private UpcomingTestsController _controller;

    [SetUp]
    public void Setup()
    {
        _studentRepositoryMock = new Mock<IStudentRepository>();
        _testRepositoryMock = new Mock<ITestRepository>();
        _controller = new UpcomingTestsController(_studentRepositoryMock.Object, _testRepositoryMock.Object);
    }

    private void SetupUser(object? userId)
    {
        Claim[] claims = [new(JwtRegisteredClaimNames.Sub, userId?.ToString() ?? "")];
        ClaimsPrincipal user = new(new ClaimsIdentity(claims, "TestAuth"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Test]
    public async Task GetUpcomingTests_ReturnsOkWithTests()
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

        List<UpcomingTestResponse> upcomingTests =
        [
            new() { TestId = 1, Name = "Test 1", Subject = "Math", NumberOfQuestions = 10 },
            new() { TestId = 2, Name = "Test 2", Subject = "English", NumberOfQuestions = 5 }
        ];

        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync(student);
        _testRepositoryMock.Setup(repo => repo.GetUpcomingTestsForStudentAsync(studentId)).ReturnsAsync(upcomingTests);

        ActionResult<IEnumerable<UpcomingTestResponse>> result = await _controller.GetUpcomingTests();

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(upcomingTests));
    }

    [Test]
    public async Task GetUpcomingTests_WhenStudentIdInvalid_ReturnsBadRequest()
    {
        const string userId = "invalid";
        SetupUser(userId);

        ActionResult<IEnumerable<UpcomingTestResponse>> result = await _controller.GetUpcomingTests();

        Assert.That(result.Result, Is.TypeOf<BadRequestResult>());
    }

    [Test]
    public async Task GetUpcomingTests_WhenStudentNotFound_ReturnsNotFound()
    {
        const int userId = 1;
        SetupUser(userId);
        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync((Core.Models.Student?)null);

        ActionResult<IEnumerable<UpcomingTestResponse>> result = await _controller.GetUpcomingTests();

        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }
}
