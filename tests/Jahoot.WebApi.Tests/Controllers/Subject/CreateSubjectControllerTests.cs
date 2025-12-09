using System.Reflection;
using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Controllers.Subject;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Subject;

public class CreateSubjectControllerTests
{
    private Mock<ISubjectRepository> _subjectRepositoryMock;
    private CreateSubjectController _controller;

    [SetUp]
    public void Setup()
    {
        _subjectRepositoryMock = new Mock<ISubjectRepository>();
        _controller = new CreateSubjectController(_subjectRepositoryMock.Object);
    }

    [Test]
    public async Task CreateSubject_InvalidModelState_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("Name", "Name is required");
        CreateSubjectRequestModel request = new() { Name = "" };

        IActionResult result = await _controller.CreateSubject(request);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        _subjectRepositoryMock.Verify(repo => repo.CreateSubjectAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CreateSubject_SubjectExists_ReturnsConflict()
    {
        CreateSubjectRequestModel request = new() { Name = "Maths" };
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByNameAsync(request.Name))
            .ReturnsAsync(new Core.Models.Subject { SubjectId = 1, Name = "Maths" });

        IActionResult result = await _controller.CreateSubject(request);

        Assert.That(result, Is.TypeOf<ConflictObjectResult>());
        ConflictObjectResult conflictResult = (ConflictObjectResult)result;
        Assert.That(conflictResult.Value, Is.EqualTo("Subject with this name already exists."));
        _subjectRepositoryMock.Verify(repo => repo.CreateSubjectAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CreateSubject_ValidRequest_ReturnsCreated()
    {
        CreateSubjectRequestModel request = new() { Name = "Science" };
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByNameAsync(request.Name))
            .ReturnsAsync((Core.Models.Subject?)null);

        IActionResult result = await _controller.CreateSubject(request);

        Assert.That(result, Is.TypeOf<StatusCodeResult>());
        StatusCodeResult statusCodeResult = (StatusCodeResult)result;
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(201));
        _subjectRepositoryMock.Verify(repo => repo.CreateSubjectAsync(request.Name), Times.Once);
    }

    [Test]
    public void CreateSubject_HasAdminAuthorization()
    {
        MethodInfo? methodInfo = typeof(CreateSubjectController).GetMethod(nameof(CreateSubjectController.CreateSubject));
        object[]? attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        Assert.That(attributes, Is.Not.Null);
        AuthorizeAttribute? authorizeAttribute = attributes.OfType<AuthorizeAttribute>().FirstOrDefault(a => a.Policy == nameof(Role.Admin));
        Assert.That(authorizeAttribute, Is.Not.Null, "Method should have [Authorize(Policy = \"Admin\")]");
    }
}
