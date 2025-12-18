using System.Reflection;
using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Controllers.Student;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Jahoot.WebApi.Services.Background;
using StudentModel = Jahoot.Core.Models.Student;

namespace Jahoot.WebApi.Tests.Controllers.Student;

public class UpdateStudentControllerTests
{
    private Mock<IStudentRepository> _studentRepositoryMock;
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<ISubjectRepository> _subjectRepositoryMock;
    private Mock<IEmailQueue> _emailQueueMock;
    private UpdateStudentController _controller;

    [SetUp]
    public void Setup()
    {
        _studentRepositoryMock = new Mock<IStudentRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _subjectRepositoryMock = new Mock<ISubjectRepository>();
        _emailQueueMock = new Mock<IEmailQueue>();
        _controller = new UpdateStudentController(_studentRepositoryMock.Object, _userRepositoryMock.Object, _subjectRepositoryMock.Object, _emailQueueMock.Object);
    }

    private void SetupUserClaims(params Role[] roles)
    {
        Claim[] claims = roles.Select(role => new Claim(ClaimTypes.Role, role.ToString())).ToArray();
        ClaimsPrincipal user = new(new ClaimsIdentity(claims, "TestAuth"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Test]
    public async Task UpdateStudent_WithValidData_ReturnsOkAndUpdates()
    {
        SetupUserClaims(Role.Lecturer);
        const int userId = 1;
        const int subjectId = 10;
        UpdateStudentRequestModel requestModel = new()
        {
            Name = "New Name",
            Email = "new@test.com",
            IsApproved = true,
            IsDisabled = false,
            SubjectIds = [subjectId]
        };

        StudentModel student = new()
        {
            UserId = userId,
            Email = "old@test.com",
            Name = "Old Name",
            PasswordHash = "hash",
            Roles = new List<Role> { Role.Student },
            StudentId = 101,
            IsApproved = false,
            IsDisabled = false,
            Subjects = []
        };

        Jahoot.Core.Models.Subject subject = new() { SubjectId = subjectId, Name = "Test Subject" };

        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync(student);
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(requestModel.Email)).ReturnsAsync((User?)null);
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectsByIdsAsync(It.Is<IEnumerable<int>>(ids => ids.Contains(subjectId)))).ReturnsAsync([subject]);

        IActionResult result = await _controller.UpdateStudent(userId, requestModel);

        Assert.That(result, Is.TypeOf<OkResult>());

        _studentRepositoryMock.Verify(repo => repo.UpdateStudentAsync(It.Is<StudentModel>(s =>
            s.IsApproved == requestModel.IsApproved &&
            s.IsDisabled == requestModel.IsDisabled &&
            s.Subjects.Count == 1 &&
            s.Subjects[0].SubjectId == subjectId)), Times.Once);

        _userRepositoryMock.Verify(repo => repo.UpdateUserAsync(It.Is<StudentModel>(s =>
            s.Name == requestModel.Name &&
            s.Email == requestModel.Email)), Times.Once);
    }

    [Test]
    public async Task UpdateStudent_StudentNotFound_ReturnsNotFound()
    {
        SetupUserClaims(Role.Lecturer);
        const int userId = 1;
        UpdateStudentRequestModel requestModel = new()
        {
            Name = "New Name",
            Email = "new@test.com",
            IsApproved = true,
            IsDisabled = false,
            SubjectIds = []
        };

        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync((StudentModel?)null);

        IActionResult result = await _controller.UpdateStudent(userId, requestModel);

        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task UpdateStudent_EmailConflict_ReturnsConflict()
    {
        SetupUserClaims(Role.Lecturer);
        const int userId = 1;
        UpdateStudentRequestModel requestModel = new()
        {
            Name = "New Name",
            Email = "taken@test.com",
            IsApproved = true,
            IsDisabled = false,
            SubjectIds = []
        };

        StudentModel student = new()
        {
            UserId = userId,
            Email = "old@test.com",
            Name = "Old Name",
            PasswordHash = "hash",
            Roles = new List<Role> { Role.Student },
            StudentId = 101,
            IsApproved = false,
            IsDisabled = false,
            Subjects = []
        };

        User otherUser = new()
        {
            UserId = 2, // Different user
            Email = "taken@test.com",
            Name = "Other User",
            PasswordHash = "hash",
            Roles = new List<Role> { Role.Student },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync(student);
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(requestModel.Email)).ReturnsAsync(otherUser);

        IActionResult result = await _controller.UpdateStudent(userId, requestModel);

        Assert.That(result, Is.TypeOf<ConflictObjectResult>());
    }

    [Test]
    public async Task UpdateStudent_ApprovedStudentCannotBeUnapproved_ReturnsBadRequest()
    {
        SetupUserClaims(Role.Lecturer);
        const int userId = 1;
        UpdateStudentRequestModel requestModel = new()
        {
            Name = "New Name",
            Email = "new@test.com",
            IsApproved = false,
            IsDisabled = false,
            SubjectIds = []
        };

        StudentModel student = new()
        {
            UserId = userId,
            Email = "old@test.com",
            Name = "Old Name",
            PasswordHash = "hash",
            Roles = new List<Role> { Role.Student },
            StudentId = 101,
            IsApproved = true, // Student is already approved
            IsDisabled = false,
            Subjects = []
        };

        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync(student);

        IActionResult result = await _controller.UpdateStudent(userId, requestModel);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That((result as BadRequestObjectResult)?.Value, Is.EqualTo("An approved student cannot be unapproved."));
        }
        _emailQueueMock.Verify(queue => queue.QueueBackgroundEmailAsync(It.IsAny<EmailMessage>()), Times.Never); // No email should be sent
    }

    [Test]
    public async Task UpdateStudent_ApprovingStudentSendsEmail_EmailsQueued()
    {
        SetupUserClaims(Role.Lecturer);
        const int userId = 1;
        const int subjectId = 10;
        UpdateStudentRequestModel requestModel = new()
        {
            Name = "Approved Student",
            Email = "approved@test.com",
            IsApproved = true, // Approving the student
            IsDisabled = false,
            SubjectIds = [subjectId]
        };

        StudentModel student = new()
        {
            UserId = userId,
            Email = "pending@test.com",
            Name = "Pending Student",
            PasswordHash = "hash",
            Roles = new List<Role> { Role.Student },
            StudentId = 101,
            IsApproved = false, // Student is initially not approved
            IsDisabled = false,
            Subjects = []
        };
        Jahoot.Core.Models.Subject subject = new() { SubjectId = subjectId, Name = "Test Subject" };

        _studentRepositoryMock.Setup(repo => repo.GetStudentByUserIdAsync(userId)).ReturnsAsync(student);
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(requestModel.Email)).ReturnsAsync((User?)null);
        _subjectRepositoryMock.Setup(repo => repo.GetSubjectsByIdsAsync(It.Is<IEnumerable<int>>(ids => ids.Contains(subjectId)))).ReturnsAsync([subject]);
        _studentRepositoryMock.Setup(repo => repo.UpdateStudentAsync(It.IsAny<StudentModel>())).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(repo => repo.UpdateUserAsync(It.IsAny<StudentModel>())).Returns(Task.CompletedTask);
        _emailQueueMock.Setup(queue => queue.QueueBackgroundEmailAsync(It.IsAny<EmailMessage>())).Returns(ValueTask.CompletedTask);

        IActionResult result = await _controller.UpdateStudent(userId, requestModel);

        Assert.That(result, Is.TypeOf<OkResult>());
        _emailQueueMock.Verify(queue => queue.QueueBackgroundEmailAsync(It.Is<EmailMessage>(
            m => m.To == requestModel.Email &&
                 m.Subject == "Jahoot Account Approved" &&
                 m.Title == "Welcome to Jahoot!" &&
                 m.Body.Contains("Your Jahoot account has been approved.")
        )), Times.Once);

        _studentRepositoryMock.Verify(repo => repo.UpdateStudentAsync(It.Is<StudentModel>(s => s.IsApproved && !s.IsDisabled)), Times.Once);
    }

    [Test]
    public void UpdateStudent_RequiresLecturerAuthorization()
    {
        MethodInfo? methodInfo = typeof(UpdateStudentController).GetMethod(nameof(UpdateStudentController.UpdateStudent));
        object[]? attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        Assert.That(attributes, Is.Not.Null);

        AuthorizeAttribute? authorizeAttribute = attributes.OfType<AuthorizeAttribute>().FirstOrDefault(a => a.Policy == nameof(Role.Lecturer));
        Assert.That(authorizeAttribute, Is.Not.Null, "Method should have [Authorize(Policy = \"Lecturer\")]");
    }
}
