using System.Reflection;
using Jahoot.Core.Models;
using Jahoot.WebApi.Controllers.Test;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Test;

public class DeleteTestControllerTests
{
    private Mock<ITestRepository> _testRepositoryMock;
    private DeleteTestController _controller;

    [SetUp]
    public void Setup()
    {
        _testRepositoryMock = new Mock<ITestRepository>();
        _controller = new DeleteTestController(_testRepositoryMock.Object);
    }

    [Test]
    public async Task DeleteTest_TestNotFound_ReturnsNotFound()
    {
        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Core.Models.Test?)null);

        IActionResult result = await _controller.DeleteTest(1);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
        _testRepositoryMock.Verify(repo => repo.DeleteTestAsync(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task DeleteTest_TestExists_ReturnsOkAndDeletesTest()
    {
        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(1))
            .ReturnsAsync(new Core.Models.Test { TestId = 1, Name = "Test", Questions = [] });

        IActionResult result = await _controller.DeleteTest(1);

        Assert.That(result, Is.TypeOf<OkResult>());
        _testRepositoryMock.Verify(repo => repo.DeleteTestAsync(1), Times.Once);
    }

    [Test]
    public void DeleteTest_RequiresLecturerAuthorization()
    {
        MethodInfo? methodInfo = typeof(DeleteTestController).GetMethod(nameof(DeleteTestController.DeleteTest));
        object[]? attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        Assert.That(attributes, Is.Not.Null);
        AuthorizeAttribute? authorizeAttribute = attributes.OfType<AuthorizeAttribute>().FirstOrDefault(a => a.Policy == nameof(Role.Lecturer));
        Assert.That(authorizeAttribute, Is.Not.Null, "Method should have [Authorize(Policy = \"Lecturer\")]");
    }
}
