using System.Reflection;
using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Controllers.Subject;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Subject;

public class UpdateSubjectControllerTests
{
    private Mock<ISubjectRepository> _subjectRepositoryMock;
    private UpdateSubjectController _controller;

    [SetUp]
    public void Setup()
    {
        _subjectRepositoryMock = new Mock<ISubjectRepository>();
        _controller = new UpdateSubjectController(_subjectRepositoryMock.Object);
    }

    [Test]
    public async Task UpdateSubject_InvalidModelState_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("Name", "Name is required");
        UpdateSubjectRequestModel request = new() { Name = "", IsActive = true };

        IActionResult result = await _controller.UpdateSubject(1, request);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        _subjectRepositoryMock.Verify(repo => repo.UpdateSubjectAsync(It.IsAny<Core.Models.Subject>()), Times.Never);
    }

    [Test]
    public async Task UpdateSubject_SubjectNotFound_ReturnsNotFound()
    {
        UpdateSubjectRequestModel request = new() { Name = "Maths", IsActive = true };
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(1)).ReturnsAsync((Core.Models.Subject?)null);

        IActionResult result = await _controller.UpdateSubject(1, request);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
        _subjectRepositoryMock.Verify(repo => repo.UpdateSubjectAsync(It.IsAny<Core.Models.Subject>()), Times.Never);
    }

    [Test]
    public async Task UpdateSubject_DuplicateName_ReturnsConflict()
    {
        UpdateSubjectRequestModel request = new() { Name = "Science", IsActive = true };
        Core.Models.Subject existingSubject = new() { SubjectId = 1, Name = "Maths" };
        Core.Models.Subject duplicateSubject = new() { SubjectId = 2, Name = "Science" };

        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(1)).ReturnsAsync(existingSubject);
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByNameAsync(request.Name)).ReturnsAsync(duplicateSubject);

        IActionResult result = await _controller.UpdateSubject(1, request);

        Assert.That(result, Is.TypeOf<ConflictObjectResult>());
        ConflictObjectResult conflictResult = (ConflictObjectResult)result;
        Assert.That(conflictResult.Value, Is.EqualTo("Subject with this name already exists."));
        _subjectRepositoryMock.Verify(repo => repo.UpdateSubjectAsync(It.IsAny<Core.Models.Subject>()), Times.Never);
    }

    [Test]
    public async Task UpdateSubject_ValidRequest_ReturnsOkAndUpdatesSubject()
    {
        UpdateSubjectRequestModel request = new() { Name = "Advanced Maths", IsActive = false };
        Core.Models.Subject existingSubject = new() { SubjectId = 1, Name = "Maths", IsActive = true };

        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(1)).ReturnsAsync(existingSubject);
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByNameAsync(request.Name)).ReturnsAsync((Core.Models.Subject?)null);

        IActionResult result = await _controller.UpdateSubject(1, request);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.TypeOf<OkResult>());
            Assert.That(existingSubject.Name, Is.EqualTo(request.Name));
            Assert.That(existingSubject.IsActive, Is.EqualTo(request.IsActive));
        }
        _subjectRepositoryMock.Verify(repo => repo.UpdateSubjectAsync(existingSubject), Times.Once);
    }

    [Test]
    public void UpdateSubject_HasAdminAuthorization()
    {
        MethodInfo? methodInfo = typeof(UpdateSubjectController).GetMethod(nameof(UpdateSubjectController.UpdateSubject));
        object[]? attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        Assert.That(attributes, Is.Not.Null);
        AuthorizeAttribute? authorizeAttribute = attributes.OfType<AuthorizeAttribute>().FirstOrDefault(a => a.Policy == nameof(Role.Admin));
        Assert.That(authorizeAttribute, Is.Not.Null, "Method should have [Authorize(Policy = \"Admin\")]");
    }
}
