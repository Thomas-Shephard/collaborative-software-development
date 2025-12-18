using Jahoot.WebApi.Controllers.Leaderboard;
using Jahoot.WebApi.Models.Responses;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Leaderboard;

public class GetSubjectLeaderboardControllerTests
{
    private Mock<ISubjectRepository> _subjectRepositoryMock;
    private GetSubjectLeaderboardController _controller;

    [SetUp]
    public void Setup()
    {
        _subjectRepositoryMock = new Mock<ISubjectRepository>();
        _controller = new GetSubjectLeaderboardController(_subjectRepositoryMock.Object);
    }

    [Test]
    public async Task GetLeaderboard_ReturnsOkWithLeaderboard_WhenSubjectExists()
    {
        const int subjectId = 1;
        Core.Models.Subject subject = new() { SubjectId = subjectId, Name = "Math", IsActive = true };
        List<LeaderboardEntry> leaderboard =
        [
            new() { Rank = 1, StudentName = "Alice", TotalScore = 100 },
            new() { Rank = 2, StudentName = "Bob", TotalScore = 90 }
        ];

        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(subjectId))
            .ReturnsAsync(subject);
        _subjectRepositoryMock.Setup(repo => repo.GetLeaderboardForSubjectAsync(subjectId))
            .ReturnsAsync(leaderboard);

        IActionResult result = await _controller.GetLeaderboard(subjectId);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        OkObjectResult? okResult = result as OkObjectResult;
        Assert.That(okResult!.Value, Is.EqualTo(leaderboard));
    }

    [Test]
    public async Task GetLeaderboard_ReturnsNotFound_WhenSubjectDoesNotExist()
    {
        const int subjectId = 99;
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(subjectId))
            .ReturnsAsync((Core.Models.Subject?)null);

        IActionResult result = await _controller.GetLeaderboard(subjectId);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult? notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult!.Value, Is.EqualTo("No subject with that id exists."));
    }

    [Test]
    public async Task GetLeaderboard_ReturnsForbidden_WhenSubjectIsDisabled()
    {
        const int subjectId = 2;
        Core.Models.Subject subject = new() { SubjectId = subjectId, Name = "DisabledMath", IsActive = false };

        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(subjectId))
            .ReturnsAsync(subject);

        IActionResult result = await _controller.GetLeaderboard(subjectId);

        Assert.That(result, Is.TypeOf<ObjectResult>());
        ObjectResult? objectResult = result as ObjectResult;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(objectResult!.StatusCode, Is.EqualTo(403));
            Assert.That(objectResult.Value, Is.EqualTo("The subject is disabled."));
        }
    }
}
