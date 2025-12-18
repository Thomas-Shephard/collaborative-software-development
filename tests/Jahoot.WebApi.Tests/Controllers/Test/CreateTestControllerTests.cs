using System.Reflection;
using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Controllers.Test;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Test;

public class CreateTestControllerTests
{
    private Mock<ITestRepository> _testRepositoryMock;
    private Mock<ISubjectRepository> _subjectRepositoryMock;
    private CreateTestController _controller;

    [SetUp]
    public void Setup()
    {
        _testRepositoryMock = new Mock<ITestRepository>();
        _subjectRepositoryMock = new Mock<ISubjectRepository>();
        _controller = new CreateTestController(_testRepositoryMock.Object, _subjectRepositoryMock.Object);
    }

    [Test]
    public async Task CreateTest_InvalidModelState_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("Name", "Name is required");
        TestRequestModel request = new() { Name = "", SubjectId = 1, Questions = [] };

        IActionResult result = await _controller.CreateTest(request);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        _testRepositoryMock.Verify(repo => repo.CreateTestAsync(It.IsAny<Core.Models.Test>()), Times.Never);
    }

    [Test]
    public async Task CreateTest_SubjectNotFound_ReturnsBadRequest()
    {
        TestRequestModel request = new() { Name = "Test 1", SubjectId = 99, Questions = [] };
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(request.SubjectId))
            .ReturnsAsync((Core.Models.Subject?)null);

        IActionResult result = await _controller.CreateTest(request);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;
        Assert.That(badRequestResult.Value, Is.EqualTo($"Subject with ID {request.SubjectId} does not exist."));
        _testRepositoryMock.Verify(repo => repo.CreateTestAsync(It.IsAny<Core.Models.Test>()), Times.Never);
    }

    [Test]
    public async Task CreateTest_ValidRequest_ReturnsCreatedAndCreatesTest()
    {
        TestRequestModel request = new()
        {
            Name = "Unit Test",
            SubjectId = 1,
            NumberOfQuestions = 1,
            Questions =
            [
                new QuestionRequestModel
                {
                    Text = "What is 2+2?",
                    Options =
                    [
                        new QuestionOptionRequestModel { OptionText = "4", IsCorrect = true },
                        new QuestionOptionRequestModel { OptionText = "5", IsCorrect = false }
                    ]
                }
            ]
        };

        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(request.SubjectId))
            .ReturnsAsync(new Core.Models.Subject { SubjectId = 1, Name = "Maths" });

        IActionResult result = await _controller.CreateTest(request);

        Assert.That(result, Is.TypeOf<CreatedResult>());
        _testRepositoryMock.Verify(repo => repo.CreateTestAsync(It.Is<Core.Models.Test>(t =>
            t.Name == request.Name &&
            t.SubjectId == request.SubjectId &&
            t.NumberOfQuestions == request.NumberOfQuestions &&
            t.Questions.Count == 1 &&
            t.Questions[0].Text == "What is 2+2?" &&
            t.Questions[0].Options.Count == 2
        )), Times.Once);
    }

    [Test]
    public void CreateTest_RequiresLecturerAuthorization()
    {
        MethodInfo? methodInfo = typeof(CreateTestController).GetMethod(nameof(CreateTestController.CreateTest));
        object[]? attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        Assert.That(attributes, Is.Not.Null);
        AuthorizeAttribute? authorizeAttribute = attributes.OfType<AuthorizeAttribute>().FirstOrDefault(a => a.Policy == nameof(Role.Lecturer));
        Assert.That(authorizeAttribute, Is.Not.Null, "Method should have [Authorize(Policy = \"Lecturer\")]");
    }
}
