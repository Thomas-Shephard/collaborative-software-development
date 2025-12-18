using System.Reflection;
using System.Security.Claims;
using Jahoot.Core.Models;
using Jahoot.WebApi.Controllers.Test;
using Jahoot.WebApi.Models.Responses.Test;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Test;

public class GetTestControllerTests
{
    private Mock<ITestRepository> _testRepositoryMock;
    private Mock<ISubjectRepository> _subjectRepositoryMock;
    private Mock<IStudentRepository> _studentRepositoryMock;
    private GetTestController _controller;

    [SetUp]
    public void Setup()
    {
        _testRepositoryMock = new Mock<ITestRepository>();
        _subjectRepositoryMock = new Mock<ISubjectRepository>();
        _studentRepositoryMock = new Mock<IStudentRepository>();
        _controller = new GetTestController(_testRepositoryMock.Object, _subjectRepositoryMock.Object, _studentRepositoryMock.Object);
    }

    private void SetupUserContext(string? userId)
    {
        List<Claim> claims = [];
        if (userId != null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        }
        ClaimsIdentity identity = new(claims, "TestAuth");
        ClaimsPrincipal claimsPrincipal = new(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Test]
    public async Task GetTest_ValidIdAndEnrolled_ReturnsTestWithNoAnswers()
    {
        const int testId = 1;
        const int subjectId = 10;
        const int userId = 123;

        SetupUserContext(userId.ToString());

        List<Question> questions =
        [
            new()
            {
                QuestionId = 1,
                Text = "Q1",
                Options = new List<QuestionOption>
                {
                    new() { QuestionOptionId = 1, OptionText = "O1", IsCorrect = true },
                    new() { QuestionOptionId = 2, OptionText = "O2", IsCorrect = false }
                }.AsReadOnly()
            }
        ];

        Core.Models.Test test = new()
        {
            TestId = testId,
            SubjectId = subjectId,
            Name = "Test 1",
            NumberOfQuestions = 1,
            Questions = questions.AsReadOnly()
        };

        Core.Models.Subject subject = new() { SubjectId = subjectId, Name = "Math", IsActive = true };

        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(testId)).ReturnsAsync(test);
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(subjectId)).ReturnsAsync(subject);
        _studentRepositoryMock.Setup(repo => repo.IsUserEnrolledInSubjectAsync(userId, subjectId)).ReturnsAsync(true);

        ActionResult<StudentTestDetailResponse> result = await _controller.GetTest(testId);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        OkObjectResult? okResult = result.Result as OkObjectResult;
        StudentTestDetailResponse? response = okResult?.Value as StudentTestDetailResponse;

        Assert.That(response, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(response!.TestId, Is.EqualTo(testId));
            Assert.That(response.SubjectName, Is.EqualTo("Math"));
            Assert.That(response.Questions.Count(), Is.EqualTo(1));
        }

        StudentQuestionResponse question = response.Questions.First();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(question.Text, Is.EqualTo("Q1"));
            Assert.That(question.Options.Count(), Is.EqualTo(2));
            
            PropertyInfo? isCorrectProperty = typeof(StudentQuestionOptionResponse).GetProperty("IsCorrect");
            Assert.That(isCorrectProperty, Is.Null, "StudentQuestionOptionResponse should not leak the IsCorrect property");
        }
    }

    [Test]
    public async Task GetTest_NotFound_ReturnsNotFound()
    {
        SetupUserContext("123");
        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(It.IsAny<int>())).ReturnsAsync((Core.Models.Test?)null);
        ActionResult<StudentTestDetailResponse> result = await _controller.GetTest(1);
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task GetTest_NotEnrolled_ReturnsForbidden()
    {
        const int testId = 1;
        const int subjectId = 10;
        const int userId = 123;

        SetupUserContext(userId.ToString());

        Core.Models.Test test = new() { TestId = testId, SubjectId = subjectId, Name = "Test 1", Questions = [] };
        Core.Models.Subject subject = new() { SubjectId = subjectId, Name = "Math", IsActive = true };

        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(testId)).ReturnsAsync(test);
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(subjectId)).ReturnsAsync(subject);
        _studentRepositoryMock.Setup(repo => repo.IsUserEnrolledInSubjectAsync(userId, subjectId)).ReturnsAsync(false);

        ActionResult<StudentTestDetailResponse> result = await _controller.GetTest(testId);

        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        ObjectResult? objectResult = result.Result as ObjectResult;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(objectResult?.StatusCode, Is.EqualTo(403));
            Assert.That(objectResult?.Value, Is.EqualTo("You are not enrolled in this subject."));
        }
    }

    [Test]
    public async Task GetTest_InvalidUserId_ReturnsBadRequest()
    {
        SetupUserContext("invalid-id");

        const int testId = 1;
        const int subjectId = 10;
        Core.Models.Test test = new() { TestId = testId, SubjectId = subjectId, Name = "Test 1", Questions = [] };
        Core.Models.Subject subject = new() { SubjectId = subjectId, Name = "Math" };

        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(testId)).ReturnsAsync(test);
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(subjectId)).ReturnsAsync(subject);

        ActionResult<StudentTestDetailResponse> result = await _controller.GetTest(testId);

        Assert.That(result.Result, Is.TypeOf<BadRequestResult>());
    }

    [Test]
    public async Task GetTest_DisabledSubject_ReturnsForbidden()
    {
        const int testId = 1;
        const int subjectId = 10;
        const int userId = 123;

        SetupUserContext(userId.ToString());

        Core.Models.Test test = new()
        {
            TestId = testId,
            SubjectId = subjectId,
            Name = "Test 1",
            NumberOfQuestions = 1,
            Questions = []
        };
        Core.Models.Subject subject = new() { SubjectId = subjectId, Name = "Math", IsActive = false };

        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(testId)).ReturnsAsync(test);
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(subjectId)).ReturnsAsync(subject);
        _studentRepositoryMock.Setup(repo => repo.IsUserEnrolledInSubjectAsync(userId, subjectId)).ReturnsAsync(true);

        ActionResult<StudentTestDetailResponse> result = await _controller.GetTest(testId);

        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        ObjectResult? objectResult = result.Result as ObjectResult;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(objectResult?.StatusCode, Is.EqualTo(403));
            Assert.That(objectResult?.Value, Is.EqualTo("The subject is disabled."));
        }
    }

    [Test]
    public void GetTest_RequiresStudentAuthorization()
    {
        MethodInfo? methodInfo = typeof(GetTestController).GetMethod(nameof(GetTestController.GetTest));
        object[]? attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        Assert.That(attributes, Is.Not.Null);
        AuthorizeAttribute? authAttr = attributes.OfType<AuthorizeAttribute>().FirstOrDefault(a => a.Policy == nameof(Role.Student));
        Assert.That(authAttr, Is.Not.Null, "Should require Student policy");
    }
}
