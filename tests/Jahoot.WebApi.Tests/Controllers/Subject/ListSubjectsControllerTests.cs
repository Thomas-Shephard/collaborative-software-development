using System.Reflection;
using Jahoot.WebApi.Controllers.Subject;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Subject;

public class ListSubjectsControllerTests
{
    private Mock<ISubjectRepository> _subjectRepositoryMock;
    private ListSubjectsController _controller;

    [SetUp]
    public void Setup()
    {
        _subjectRepositoryMock = new Mock<ISubjectRepository>();
        _controller = new ListSubjectsController(_subjectRepositoryMock.Object);
    }

    [Test]
    public async Task ListSubjects_ReturnsOkWithSubjects()
    {
        List<Core.Models.Subject> subjects =
        [
            new() { SubjectId = 1, Name = "Maths" },
            new() { SubjectId = 2, Name = "Science" }
        ];

        _subjectRepositoryMock.Setup(repo => repo.GetAllSubjectsAsync()).ReturnsAsync(subjects);

        IActionResult result = await _controller.ListSubjects();

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        OkObjectResult okResult = (OkObjectResult)result;
        Core.Models.Subject[]? returnedSubjects = (okResult.Value as IEnumerable<Core.Models.Subject>)?.ToArray();

        Assert.That(returnedSubjects, Is.Not.Null);
        Assert.That(returnedSubjects, Has.Length.EqualTo(2));
    }

    [Test]
    public async Task ListSubjects_WithoutIsActive_CallsRepositoryWithNull()
    {
        _subjectRepositoryMock.Setup(repo => repo.GetAllSubjectsAsync(null)).ReturnsAsync(new List<Core.Models.Subject>());

        await _controller.ListSubjects();

        _subjectRepositoryMock.Verify(repo => repo.GetAllSubjectsAsync(null), Times.Once);
    }

    [Test]
    public async Task ListSubjects_WithIsActiveTrue_CallsRepositoryWithTrue()
    {
        _subjectRepositoryMock.Setup(repo => repo.GetAllSubjectsAsync(true)).ReturnsAsync(new List<Core.Models.Subject>());

        await _controller.ListSubjects(true);

        _subjectRepositoryMock.Verify(repo => repo.GetAllSubjectsAsync(true), Times.Once);
    }

    [Test]
    public async Task ListSubjects_WithIsActiveFalse_CallsRepositoryWithFalse()
    {
        _subjectRepositoryMock.Setup(repo => repo.GetAllSubjectsAsync(false)).ReturnsAsync(new List<Core.Models.Subject>());

        await _controller.ListSubjects(false);

        _subjectRepositoryMock.Verify(repo => repo.GetAllSubjectsAsync(false), Times.Once);
    }

    [Test]
    public void ListSubjects_HasAuthorization()
    {
        MethodInfo? methodInfo = typeof(ListSubjectsController).GetMethod(nameof(ListSubjectsController.ListSubjects));
        object[]? attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        Assert.That(attributes, Is.Not.Null);
        Assert.That(attributes, Is.Not.Empty, "Method should be decorated with [Authorize]");
    }
}
