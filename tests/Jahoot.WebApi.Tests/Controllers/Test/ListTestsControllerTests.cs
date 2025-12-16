using System.Reflection;
using Jahoot.Core.Models;
using Jahoot.WebApi.Controllers.Test;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Test;

public class ListTestsControllerTests
{
    private Mock<ITestRepository> _testRepositoryMock;
    private ListTestsController _controller;

    [SetUp]
    public void Setup()
    {
        _testRepositoryMock = new Mock<ITestRepository>();
        _controller = new ListTestsController(_testRepositoryMock.Object);
    }

    [Test]
    public async Task ListTests_NoSubjectId_ReturnsAllTests()
    {
        List<Core.Models.Test> tests =
        [
            new() { TestId = 1, Name = "Test 1", Questions = [] },
            new() { TestId = 2, Name = "Test 2", Questions = [] }
        ];

        _testRepositoryMock.Setup(repo => repo.GetAllTestsAsync(null)).ReturnsAsync(tests);

        IActionResult result = await _controller.ListTests(null);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        OkObjectResult okResult = (OkObjectResult)result;
        Core.Models.Test[]? returnedTests = (okResult.Value as IEnumerable<Core.Models.Test>)?.ToArray();
        Assert.That(returnedTests, Is.Not.Null);
        Assert.That(returnedTests, Has.Length.EqualTo(2));
    }

    [Test]
    public async Task ListTests_WithSubjectId_ReturnsFilteredTests()
    {
        int subjectId = 1;
        List<Core.Models.Test> tests =
        [
            new() { TestId = 1, Name = "Test 1", SubjectId = subjectId, Questions = [] }
        ];

        _testRepositoryMock.Setup(repo => repo.GetAllTestsAsync(subjectId)).ReturnsAsync(tests);

        IActionResult result = await _controller.ListTests(subjectId);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        OkObjectResult okResult = (OkObjectResult)result;
        Core.Models.Test[]? returnedTests = (okResult.Value as IEnumerable<Core.Models.Test>)?.ToArray();
        Assert.That(returnedTests, Is.Not.Null);
        Assert.That(returnedTests, Has.Length.EqualTo(1));
    }

    [Test]
    public void ListTests_RequiresLecturerAuthorization()
    {
        MethodInfo? methodInfo = typeof(ListTestsController).GetMethod(nameof(ListTestsController.ListTests));
        object[]? attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        Assert.That(attributes, Is.Not.Null);
        AuthorizeAttribute? authorizeAttribute = attributes.OfType<AuthorizeAttribute>().FirstOrDefault(a => a.Policy == nameof(Role.Lecturer));
        Assert.That(authorizeAttribute, Is.Not.Null, "Method should have [Authorize(Policy = \"Lecturer\")]");
    }
}
