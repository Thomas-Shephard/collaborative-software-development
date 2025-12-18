using Jahoot.WebApi.Controllers.Lecturer;
using Jahoot.WebApi.Models.Responses;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Lecturer;

public class GetRecentStudentActivityControllerTests
{
    private Mock<ITestRepository> _testRepositoryMock;
    private GetRecentStudentActivityController _controller;

    [SetUp]
    public void Setup()
    {
        _testRepositoryMock = new Mock<ITestRepository>();
        _controller = new GetRecentStudentActivityController(_testRepositoryMock.Object);
    }

    [Test]
    public async Task GetRecentActivity_ReturnsOkWithTests()
    {
        List<CompletedTestResponse> recentTests =
        [
            new() { TestName = "Test 1", SubjectName = "Subject 1", StudentName = "Student A" },
            new() { TestName = "Test 2", SubjectName = "Subject 2", StudentName = "Student B" }
        ];

        _testRepositoryMock.Setup(repo => repo.GetRecentCompletedTestsAsync(7)).ReturnsAsync(recentTests);

        IActionResult result = await _controller.GetRecentActivity();

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        OkObjectResult? okResult = result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(recentTests));
    }

    [Test]
    public async Task GetRecentActivity_ReturnsEmptyList_WhenNoTestsFound()
    {
        _testRepositoryMock.Setup(repo => repo.GetRecentCompletedTestsAsync(7)).ReturnsAsync([]);

        IActionResult result = await _controller.GetRecentActivity();

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        OkObjectResult? okResult = result as OkObjectResult;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(okResult!.Value, Is.InstanceOf<IEnumerable<CompletedTestResponse>>());
            Assert.That((IEnumerable<CompletedTestResponse>)okResult.Value!, Is.Empty);
        }
    }
}
