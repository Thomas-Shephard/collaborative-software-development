using System.Reflection;
using Jahoot.Core.Models;
using Jahoot.WebApi.Controllers.Test;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Test;

public class GetTestAttemptsStatusControllerTests
{
    private Mock<ITestRepository> _testRepositoryMock;
    private GetTestAttemptsStatusController _controller;

    [SetUp]
    public void Setup()
    {
        _testRepositoryMock = new Mock<ITestRepository>();
        _controller = new GetTestAttemptsStatusController(_testRepositoryMock.Object);
    }

    [Test]
    public async Task HasAttempts_TestNotFound_ReturnsNotFound()
    {
        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Core.Models.Test?)null);

        IActionResult result = await _controller.HasAttempts(1);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
        _testRepositoryMock.Verify(repo => repo.HasAttemptsAsync(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task HasAttempts_TestExists_ReturnsOkWithStatus()
    {
        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(1))
            .ReturnsAsync(new Core.Models.Test { TestId = 1, Name = "Test", Questions = [] });
        _testRepositoryMock.Setup(repo => repo.HasAttemptsAsync(1))
            .ReturnsAsync(true);

        IActionResult result = await _controller.HasAttempts(1);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        OkObjectResult okResult = (OkObjectResult)result;
        bool? hasAttempts = (bool?)okResult.Value?.GetType().GetProperty("HasAttempts")?.GetValue(okResult.Value, null);
        Assert.That(hasAttempts, Is.True);
    }

    [Test]
    public void HasAttempts_RequiresLecturerAuthorization()
    {
        MethodInfo? methodInfo = typeof(GetTestAttemptsStatusController).GetMethod(nameof(GetTestAttemptsStatusController.HasAttempts));
        object[]? attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        Assert.That(attributes, Is.Not.Null);
        AuthorizeAttribute? authorizeAttribute = attributes.OfType<AuthorizeAttribute>().FirstOrDefault(a => a.Policy == nameof(Role.Lecturer));
        Assert.That(authorizeAttribute, Is.Not.Null, "Method should have [Authorize(Policy = \"Lecturer\")]");
    }
}
