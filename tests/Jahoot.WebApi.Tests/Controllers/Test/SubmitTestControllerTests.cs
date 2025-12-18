using System.Reflection;
using System.Security.Claims;
using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Controllers.Test;
using Jahoot.WebApi.Models.Responses;
using Jahoot.WebApi.Repositories;
using Jahoot.WebApi.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Test;

public class SubmitTestControllerTests
{
    private Mock<ITestRepository> _testRepositoryMock;
    private Mock<ISubjectRepository> _subjectRepositoryMock;
    private Mock<IStudentRepository> _studentRepositoryMock;
    private Mock<IOptions<ScoringSettings>> _scoringSettingsMock;
    private SubmitTestController _controller;

    [SetUp]
    public void Setup()
    {
        _testRepositoryMock = new Mock<ITestRepository>();
        _subjectRepositoryMock = new Mock<ISubjectRepository>();
        _studentRepositoryMock = new Mock<IStudentRepository>();
        _scoringSettingsMock = new Mock<IOptions<ScoringSettings>>();

        _scoringSettingsMock.Setup(s => s.Value).Returns(new ScoringSettings
        {
            PointsPerCorrectAnswer = 30,
            PointsPerIncorrectAnswer = -10
        });

        _controller = new SubmitTestController(_testRepositoryMock.Object, _subjectRepositoryMock.Object, _studentRepositoryMock.Object, _scoringSettingsMock.Object);
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
    public async Task SubmitTest_ValidSubmission_ReturnsCompletedTestResponseWithCorrectScore()
    {
        const int testId = 1;
        const int subjectId = 10;
        const int userId = 123;
        const int studentId = 456;

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
            },
            new()
            {
                QuestionId = 2,
                Text = "Q2",
                Options = new List<QuestionOption>
                {
                    new() { QuestionOptionId = 3, OptionText = "O3", IsCorrect = true },
                    new() { QuestionOptionId = 4, OptionText = "O4", IsCorrect = false }
                }.AsReadOnly()
            }
        ];

        Core.Models.Test test = new()
        {
            TestId = testId,
            SubjectId = subjectId,
            Name = "Test 1",
            NumberOfQuestions = 2,
            Questions = questions.AsReadOnly()
        };

        Core.Models.Subject subject = new() { SubjectId = subjectId, Name = "Math", IsActive = true };
        Core.Models.Student student = new() { UserId = userId, StudentId = studentId, Name = "John Doe", Email = "john@example.com", Roles = [], Subjects = [] };

        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(testId)).ReturnsAsync(test);
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(subjectId)).ReturnsAsync(subject);
        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync(student);
        _studentRepositoryMock.Setup(repo => repo.IsUserEnrolledInSubjectAsync(userId, subjectId)).ReturnsAsync(true);

        SubmitTestRequestModel request = new()
        {
            Answers =
            [
                new AnswerRequestModel { QuestionId = 1, SelectedOptionId = 1 }, // Correct: +30
                new AnswerRequestModel { QuestionId = 2, SelectedOptionId = 4 }  // Incorrect: -10
            ]
        };

        ActionResult<CompletedTestResponse> result = await _controller.SubmitTest(testId, request);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        OkObjectResult? okResult = result.Result as OkObjectResult;
        CompletedTestResponse? response = okResult?.Value as CompletedTestResponse;

        Assert.That(response, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(response!.TestId, Is.EqualTo(testId));
            Assert.That(response.TotalPoints, Is.EqualTo(20)); // 30 - 10
            Assert.That(response.ScorePercentage, Is.EqualTo(50.0)); // 1 correct / 2 total * 100
            Assert.That(response.StudentName, Is.EqualTo("John Doe"));
        }

        _testRepositoryMock.Verify(repo => repo.SaveTestResultAsync(studentId, testId, 20, 1), Times.Once);
    }

    [Test]
    public async Task SubmitTest_InvalidOptionId_DoesNotPenalize()
    {
        const int testId = 1;
        const int subjectId = 10;
        const int userId = 123;
        const int studentId = 456;

        SetupUserContext(userId.ToString());

        List<Question> questions =
        [
            new()
            {
                QuestionId = 1,
                Text = "Q1",
                Options = new List<QuestionOption>
                {
                    new() { QuestionOptionId = 1, OptionText = "O1", IsCorrect = true }
                }.AsReadOnly()
            }
        ];

        Core.Models.Test test = new() { TestId = testId, SubjectId = subjectId, Name = "Test 1", NumberOfQuestions = 1, Questions = questions.AsReadOnly() };
        Core.Models.Subject subject = new() { SubjectId = subjectId, Name = "Math", IsActive = true };
        Core.Models.Student student = new() { UserId = userId, StudentId = studentId, Name = "John Doe", Email = "john@example.com", Roles = [], Subjects = [] };

        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(testId)).ReturnsAsync(test);
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(subjectId)).ReturnsAsync(subject);
        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync(student);
        _studentRepositoryMock.Setup(repo => repo.IsUserEnrolledInSubjectAsync(userId, subjectId)).ReturnsAsync(true);

        SubmitTestRequestModel request = new()
        {
            Answers = [new AnswerRequestModel { QuestionId = 1, SelectedOptionId = 999 }] // Invalid ID
        };

        ActionResult<CompletedTestResponse> result = await _controller.SubmitTest(testId, request);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        CompletedTestResponse? response = okResult?.Value as CompletedTestResponse;

        Assert.That(response?.TotalPoints, Is.Zero);
        _testRepositoryMock.Verify(repo => repo.SaveTestResultAsync(studentId, testId, 0, 0), Times.Once);
    }

    [Test]
    public async Task SubmitTest_DuplicateQuestionAnswers_OnlyScoresFirstAnswer()
    {
        const int testId = 1;
        const int subjectId = 10;
        const int userId = 123;
        const int studentId = 456;

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

        Core.Models.Test test = new() { TestId = testId, SubjectId = subjectId, Name = "Test 1", NumberOfQuestions = 1, Questions = questions.AsReadOnly() };
        Core.Models.Subject subject = new() { SubjectId = subjectId, Name = "Math", IsActive = true };
        Core.Models.Student student = new() { UserId = userId, StudentId = studentId, Name = "John Doe", Email = "john@example.com", Roles = [], Subjects = [] };

        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(testId)).ReturnsAsync(test);
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(subjectId)).ReturnsAsync(subject);
        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync(student);
        _studentRepositoryMock.Setup(repo => repo.IsUserEnrolledInSubjectAsync(userId, subjectId)).ReturnsAsync(true);

        SubmitTestRequestModel request = new()
        {
            Answers =
            [
                new AnswerRequestModel { QuestionId = 1, SelectedOptionId = 1 }, // Correct: +30
                new AnswerRequestModel { QuestionId = 1, SelectedOptionId = 1 }  // Duplicate: Should be ignored
            ]
        };

        ActionResult<CompletedTestResponse> result = await _controller.SubmitTest(testId, request);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        CompletedTestResponse? response = okResult?.Value as CompletedTestResponse;

        Assert.That(response?.TotalPoints, Is.EqualTo(30));
        _testRepositoryMock.Verify(repo => repo.SaveTestResultAsync(studentId, testId, 30, 1), Times.Once);
    }

    [Test]
    public async Task SubmitTest_NotFound_ReturnsNotFound()
    {
        SetupUserContext("123");
        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(It.IsAny<int>())).ReturnsAsync((Core.Models.Test?)null);

        ActionResult<CompletedTestResponse> result = await _controller.SubmitTest(1, new SubmitTestRequestModel { Answers = [] });
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task SubmitTest_AlreadyCompleted_ReturnsBadRequest()
    {
        const int testId = 1;
        const int userId = 123;
        const int studentId = 456;

        SetupUserContext(userId.ToString());

        Core.Models.Test test = new() { TestId = testId, SubjectId = 10, Name = "Test 1", Questions = [] };
        Core.Models.Subject subject = new() { SubjectId = 10, Name = "Math", IsActive = true };
        Core.Models.Student student = new() { UserId = userId, StudentId = studentId, Name = "John Doe", Email = "john@example.com", Roles = [], Subjects = [] };

        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(testId)).ReturnsAsync(test);
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(10)).ReturnsAsync(subject);
        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync(student);
        _testRepositoryMock.Setup(repo => repo.HasStudentCompletedTestAsync(studentId, testId)).ReturnsAsync(true);

        ActionResult<CompletedTestResponse> result = await _controller.SubmitTest(testId, new SubmitTestRequestModel { Answers = [] });

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        BadRequestObjectResult? badRequest = result.Result as BadRequestObjectResult;
        Assert.That(badRequest?.Value, Is.EqualTo("You have already completed this test."));
    }

    [Test]
    public async Task SubmitTest_DisabledSubject_ReturnsForbidden()
    {
        const int testId = 1;
        const int subjectId = 10;
        const int userId = 123;
        const int studentId = 456;

        SetupUserContext(userId.ToString());

        Core.Models.Test test = new() { TestId = testId, SubjectId = subjectId, Name = "Test 1", Questions = [] };
        Core.Models.Subject subject = new() { SubjectId = subjectId, Name = "Math", IsActive = false };
        Core.Models.Student student = new() { UserId = userId, StudentId = studentId, Name = "John Doe", Email = "john@example.com", Roles = [], Subjects = [] };

        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(testId)).ReturnsAsync(test);
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(subjectId)).ReturnsAsync(subject);
        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync(student);
        _studentRepositoryMock.Setup(repo => repo.IsUserEnrolledInSubjectAsync(userId, subjectId)).ReturnsAsync(true);

        ActionResult<CompletedTestResponse> result = await _controller.SubmitTest(testId, new SubmitTestRequestModel { Answers = [] });

        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        ObjectResult? objectResult = result.Result as ObjectResult;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(objectResult?.StatusCode, Is.EqualTo(403));
            Assert.That(objectResult?.Value, Is.EqualTo("The subject is disabled."));
        }
    }

    [Test]
    public async Task SubmitTest_InvalidUserId_ReturnsBadRequest()
    {
        SetupUserContext("invalid-id");

        const int testId = 1;
        const int subjectId = 10;
        Core.Models.Test test = new() { TestId = testId, SubjectId = subjectId, Name = "Test 1", Questions = [] };
        Core.Models.Subject subject = new() { SubjectId = subjectId, Name = "Math" };

        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(testId)).ReturnsAsync(test);
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(subjectId)).ReturnsAsync(subject);

        ActionResult<CompletedTestResponse> result = await _controller.SubmitTest(testId, new SubmitTestRequestModel { Answers = [] });

        Assert.That(result.Result, Is.TypeOf<BadRequestResult>());
    }

    [Test]
    public async Task SubmitTest_StudentNotFound_ReturnsNotFound()
    {
        const int testId = 1;
        const int userId = 123;

        SetupUserContext(userId.ToString());

        Core.Models.Test test = new() { TestId = testId, SubjectId = 10, Name = "Test 1", Questions = [] };
        Core.Models.Subject subject = new() { SubjectId = 10, Name = "Math", IsActive = true };

        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(testId)).ReturnsAsync(test);
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(10)).ReturnsAsync(subject);
        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync((Core.Models.Student?)null);

        ActionResult<CompletedTestResponse> result = await _controller.SubmitTest(testId, new SubmitTestRequestModel { Answers = [] });

        Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult? notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult?.Value, Is.EqualTo("Student not found."));
    }

    [Test]
    public async Task SubmitTest_NotEnrolled_ReturnsForbidden()
    {
        const int testId = 1;
        const int subjectId = 10;
        const int userId = 123;
        const int studentId = 456;

        SetupUserContext(userId.ToString());

        Core.Models.Test test = new() { TestId = testId, SubjectId = subjectId, Name = "Test 1", Questions = [] };
        Core.Models.Subject subject = new() { SubjectId = subjectId, Name = "Math", IsActive = true };
        Core.Models.Student student = new() { UserId = userId, StudentId = studentId, Name = "John Doe", Email = "john@example.com", Roles = [], Subjects = [] };

        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(testId)).ReturnsAsync(test);
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(subjectId)).ReturnsAsync(subject);
        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync(student);
        _studentRepositoryMock.Setup(repo => repo.IsUserEnrolledInSubjectAsync(userId, subjectId)).ReturnsAsync(false);

        ActionResult<CompletedTestResponse> result = await _controller.SubmitTest(testId, new SubmitTestRequestModel { Answers = [] });

        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        ObjectResult? objectResult = result.Result as ObjectResult;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(objectResult?.StatusCode, Is.EqualTo(403));
            Assert.That(objectResult?.Value, Is.EqualTo("You are not enrolled in this subject."));
        }
    }

    [Test]
    public async Task SubmitTest_ZeroQuestions_ReturnsZeroScorePercentage()
    {
        const int testId = 1;
        const int subjectId = 10;
        const int userId = 123;
        const int studentId = 456;

        SetupUserContext(userId.ToString());

        Core.Models.Test test = new() { TestId = testId, SubjectId = subjectId, Name = "Test 1", NumberOfQuestions = 0, Questions = [] };
        Core.Models.Subject subject = new() { SubjectId = subjectId, Name = "Math", IsActive = true };
        Core.Models.Student student = new() { UserId = userId, StudentId = studentId, Name = "John Doe", Email = "john@example.com", Roles = [], Subjects = [] };

        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(testId)).ReturnsAsync(test);
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(subjectId)).ReturnsAsync(subject);
        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync(student);
        _studentRepositoryMock.Setup(repo => repo.IsUserEnrolledInSubjectAsync(userId, subjectId)).ReturnsAsync(true);

        ActionResult<CompletedTestResponse> result = await _controller.SubmitTest(testId, new SubmitTestRequestModel { Answers = [] });

        OkObjectResult? okResult = result.Result as OkObjectResult;
        CompletedTestResponse? response = okResult?.Value as CompletedTestResponse;

        Assert.That(response?.ScorePercentage, Is.Zero);
    }

    [Test]
    public async Task SubmitTest_InvalidQuestionId_DoesNotPenalize()
    {
        const int testId = 1;
        const int subjectId = 10;
        const int userId = 123;
        const int studentId = 456;

        SetupUserContext(userId.ToString());

        Core.Models.Test test = new() { TestId = testId, SubjectId = subjectId, Name = "Test 1", NumberOfQuestions = 1, Questions = [] };
        Core.Models.Subject subject = new() { SubjectId = subjectId, Name = "Math", IsActive = true };
        Core.Models.Student student = new() { UserId = userId, StudentId = studentId, Name = "John Doe", Email = "john@example.com", Roles = [], Subjects = [] };

        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(testId)).ReturnsAsync(test);
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(subjectId)).ReturnsAsync(subject);
        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync(student);
        _studentRepositoryMock.Setup(repo => repo.IsUserEnrolledInSubjectAsync(userId, subjectId)).ReturnsAsync(true);

        SubmitTestRequestModel request = new()
        {
            Answers = [new() { QuestionId = 999, SelectedOptionId = 1 }] // Invalid Question ID
        };

        ActionResult<CompletedTestResponse> result = await _controller.SubmitTest(testId, request);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        CompletedTestResponse? response = okResult?.Value as CompletedTestResponse;

        Assert.That(response?.TotalPoints, Is.Zero);
        _testRepositoryMock.Verify(repo => repo.SaveTestResultAsync(studentId, testId, 0, 0), Times.Once);
    }

    [Test]
    public async Task SubmitTest_EmptyAnswers_ReturnsZeroScore()
    {
        const int testId = 1;
        const int subjectId = 10;
        const int userId = 123;
        const int studentId = 456;

        SetupUserContext(userId.ToString());

        Core.Models.Test test = new() { TestId = testId, SubjectId = subjectId, Name = "Test 1", NumberOfQuestions = 1, Questions = [] };
        Core.Models.Subject subject = new() { SubjectId = subjectId, Name = "Math", IsActive = true };
        Core.Models.Student student = new() { UserId = userId, StudentId = studentId, Name = "John Doe", Email = "john@example.com", Roles = [], Subjects = [] };

        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(testId)).ReturnsAsync(test);
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(subjectId)).ReturnsAsync(subject);
        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync(student);
        _studentRepositoryMock.Setup(repo => repo.IsUserEnrolledInSubjectAsync(userId, subjectId)).ReturnsAsync(true);

        SubmitTestRequestModel request = new()
        {
            Answers = []
        };

        ActionResult<CompletedTestResponse> result = await _controller.SubmitTest(testId, request);

        OkObjectResult? okResult = result.Result as OkObjectResult;
        CompletedTestResponse? response = okResult?.Value as CompletedTestResponse;

        Assert.That(response?.TotalPoints, Is.Zero);
        _testRepositoryMock.Verify(repo => repo.SaveTestResultAsync(studentId, testId, 0, 0), Times.Once);
    }

    [Test]
    public async Task SubmitTest_SubjectNotFound_ReturnsInternalServerError()
    {
        const int testId = 1;
        const int subjectId = 10;
        const int userId = 123;

        SetupUserContext(userId.ToString());

        Core.Models.Test test = new() { TestId = testId, SubjectId = subjectId, Name = "Test 1", Questions = [] };

        _testRepositoryMock.Setup(repo => repo.GetTestByIdAsync(testId)).ReturnsAsync(test);
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectByIdAsync(subjectId)).ReturnsAsync((Core.Models.Subject?)null);

        ActionResult<CompletedTestResponse> result = await _controller.SubmitTest(testId, new SubmitTestRequestModel { Answers = [] });

        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        ObjectResult? objectResult = result.Result as ObjectResult;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
            Assert.That(objectResult?.Value, Is.EqualTo("Test has invalid subject"));
        }
    }

    [Test]
    public void SubmitTest_RequiresStudentAuthorization()
    {
        MethodInfo? methodInfo = typeof(SubmitTestController).GetMethod(nameof(SubmitTestController.SubmitTest));
        object[]? attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        Assert.That(attributes, Is.Not.Null);
        AuthorizeAttribute? authAttr = attributes.OfType<AuthorizeAttribute>().FirstOrDefault(a => a.Policy == nameof(Role.Student));
        Assert.That(authAttr, Is.Not.Null, "Should require Student policy");
    }
}
