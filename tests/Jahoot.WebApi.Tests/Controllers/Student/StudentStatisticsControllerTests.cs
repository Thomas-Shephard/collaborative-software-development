using System.Security.Claims;
using Jahoot.Core.Models;
using Jahoot.WebApi.Controllers.Student;
using Jahoot.WebApi.Models.Responses;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Student;

public class StudentStatisticsControllerTests
{
    private Mock<IStudentRepository> _studentRepositoryMock;
    private Mock<ITestRepository> _testRepositoryMock;
    private StudentStatisticsController _controller;

    [SetUp]
    public void Setup()
    {
        _studentRepositoryMock = new Mock<IStudentRepository>();
        _testRepositoryMock = new Mock<ITestRepository>();
        _controller = new StudentStatisticsController(_studentRepositoryMock.Object, _testRepositoryMock.Object);
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
    public async Task GetStatistics_ReturnsOkWithStats()
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

        StudentStatisticsResponse stats = new()
        {
            TotalPoints = 500,
            AverageScorePercentage = 85.5,
            ScoreHistory =
            [
                new() { Date = DateTime.UtcNow.AddDays(-1), ScorePercentage = 80 },
                new() { Date = DateTime.UtcNow, ScorePercentage = 90 }
            ]
        };

        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync(student);
        _testRepositoryMock.Setup(repo => repo.GetStudentStatisticsAsync(studentId)).ReturnsAsync(stats);

        ActionResult<StudentStatisticsResponse> result = await _controller.GetStatistics();

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        OkObjectResult? okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(stats));
    }

    [Test]
    public async Task GetStatistics_WhenUserIdInvalid_ReturnsBadRequest()
    {
        const string userId = "invalid";
        SetupUser(userId);

        ActionResult<StudentStatisticsResponse> result = await _controller.GetStatistics();

        Assert.That(result.Result, Is.TypeOf<BadRequestResult>());
    }

    [Test]
    public async Task GetStatistics_WhenStudentNotFound_ReturnsNotFound()
    {
        const int userId = 1;
        SetupUser(userId);
        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync((Core.Models.Student?)null);

        ActionResult<StudentStatisticsResponse> result = await _controller.GetStatistics();

        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }
}
