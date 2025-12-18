using System.Reflection;
using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Controllers.Test;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Test;

public class UpdateTestControllerTests
{
    private Mock<ITestRepository> _testRepositoryMock;
    private Mock<ISubjectRepository> _subjectRepositoryMock;
    private UpdateTestController _controller;

    [SetUp]
    public void Setup()
    {
        _testRepositoryMock = new Mock<ITestRepository>();
        _subjectRepositoryMock = new Mock<ISubjectRepository>();
        _controller = new UpdateTestController(_testRepositoryMock.Object, _subjectRepositoryMock.Object);
    }

    [Test]
    public async Task UpdateTest_InvalidModelState_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("Name", "Name is required");
        TestRequestModel request = new() { Name = "", SubjectId = 1, Questions = [] };

        IActionResult result = await _controller.UpdateTest(1, request);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        _testRepositoryMock.Verify(repo => repo.UpdateTestAsync(It.IsAny<Core.Models.Test>()), Times.Never);
    }

    [Test]
    public async Task UpdateTest_TestNotFound_ReturnsNotFound()
    {
        TestRequestModel request = new() { Name = "Updated Test", SubjectId = 1, Questions = [] };
        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(1))
            .ReturnsAsync((Core.Models.Test?)null);

        IActionResult result = await _controller.UpdateTest(1, request);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
        _testRepositoryMock.Verify(repo => repo.UpdateTestAsync(It.IsAny<Core.Models.Test>()), Times.Never);
    }

    [Test]
    public async Task UpdateTest_SubjectNotFound_ReturnsBadRequest()
    {
        TestRequestModel request = new() { Name = "Updated Test", SubjectId = 99, Questions = [] };
        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(1))
            .ReturnsAsync(new Core.Models.Test { TestId = 1, Name = "Original Test", Questions = [] });
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(request.SubjectId))
            .ReturnsAsync((Core.Models.Subject?)null);

        IActionResult result = await _controller.UpdateTest(1, request);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;
        Assert.That(badRequestResult.Value, Is.EqualTo($"Subject with ID {request.SubjectId} does not exist."));
        _testRepositoryMock.Verify(repo => repo.UpdateTestAsync(It.IsAny<Core.Models.Test>()), Times.Never);
    }

    [Test]
    public async Task UpdateTest_ValidRequest_ReturnsOkAndUpdatesTest()
    {
        int testId = 1;
        TestRequestModel request = new()
        {
            Name = "Updated Test",
            SubjectId = 1,
            NumberOfQuestions = 1,
            Questions =
            [
                new QuestionRequestModel
                {
                    Text = "Updated Question",
                    Options =
                    [
                        new QuestionOptionRequestModel { OptionText = "A", IsCorrect = true }
                    ]
                }
            ]
        };

        Core.Models.Test existingTest = new() { TestId = testId, Name = "Original Test", SubjectId = 2, Questions = [] };

        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(testId))
            .ReturnsAsync(existingTest);
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(request.SubjectId))
            .ReturnsAsync(new Core.Models.Subject { SubjectId = 1, Name = "Maths" });

        IActionResult result = await _controller.UpdateTest(testId, request);

        Assert.That(result, Is.TypeOf<OkResult>());

        _testRepositoryMock.Verify(repo => repo.UpdateTestAsync(It.Is<Core.Models.Test>(t =>
            t.TestId == testId &&
            t.Name == request.Name &&
            t.SubjectId == request.SubjectId &&
            t.NumberOfQuestions == request.NumberOfQuestions &&
            t.Questions.Count == 1 &&
            t.Questions[0].Text == "Updated Question"
        )), Times.Once);
    }

    [Test]
    public void UpdateTest_RequiresLecturerAuthorization()
    {
        MethodInfo? methodInfo = typeof(UpdateTestController).GetMethod(nameof(UpdateTestController.UpdateTest));
        object[]? attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        Assert.That(attributes, Is.Not.Null);
        AuthorizeAttribute? authorizeAttribute = attributes.OfType<AuthorizeAttribute>().FirstOrDefault(a => a.Policy == nameof(Role.Lecturer));
        Assert.That(authorizeAttribute, Is.Not.Null, "Method should have [Authorize(Policy = \"Lecturer\")]");
    }
}
