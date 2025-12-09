using System.Reflection;
using Jahoot.Core.Models;
using Jahoot.WebApi.Controllers.Student;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.AspNetCore.Authorization;

namespace Jahoot.WebApi.Tests.Controllers.Student;

public class ApproveStudentControllerTests
{
    private Mock<IStudentRepository> _mockStudentRepository;
    private ApproveStudentController _controller;

    [SetUp]
    public void Setup()
    {
        _mockStudentRepository = new Mock<IStudentRepository>();
        _controller = new ApproveStudentController(_mockStudentRepository.Object);
    }

    [Test]
    public async Task ApproveStudentRegistration_ShouldReturnOk_WhenStudentIsPendingApproval()
    {
        const int userId = 1;
        Jahoot.Core.Models.Student student = new()
        {
            UserId = userId,
            AccountStatus = StudentAccountStatus.PendingApproval,
            StudentId = 1,
            Email = "test@example.com",
            Name = "Test Student",
            PasswordHash = "hashed_password",
            Roles = [Role.Student]
        };
        _mockStudentRepository.Setup(r => r.GetStudentByUserIdAsync(userId)).ReturnsAsync(student);

        IActionResult result = await _controller.ApproveStudentRegistration(userId);

        Assert.That(result, Is.InstanceOf<OkResult>());
        _mockStudentRepository.Verify(r => r.UpdateStudentAsync(It.Is<Jahoot.Core.Models.Student>(s => s.AccountStatus == StudentAccountStatus.Active)), Times.Once);
    }

    [Test]
    public async Task ApproveStudentRegistration_ShouldReturnNotFound_WhenStudentDoesNotExist()
    {
        const int userId = 1;
        _mockStudentRepository.Setup(r => r.GetStudentByUserIdAsync(userId)).ReturnsAsync((Jahoot.Core.Models.Student?)null);

        IActionResult result = await _controller.ApproveStudentRegistration(userId);

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        NotFoundObjectResult? notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult?.Value, Is.EqualTo($"Student with user ID {userId} not found."));
        _mockStudentRepository.Verify(r => r.UpdateStudentAsync(It.IsAny<Jahoot.Core.Models.Student>()), Times.Never);
    }

    [Test]
    public async Task ApproveStudentRegistration_ShouldReturnBadRequest_WhenStudentIsNotPendingApproval()
    {
        const int userId = 1;
        Jahoot.Core.Models.Student student = new()
        {
            UserId = userId,
            AccountStatus = StudentAccountStatus.Active,
            StudentId = 1,
            Email = "test@example.com",
            Name = "Test Student",
            PasswordHash = "hashed_password",
            Roles = [Role.Student]
        };
        _mockStudentRepository.Setup(r => r.GetStudentByUserIdAsync(userId)).ReturnsAsync(student);

        IActionResult result = await _controller.ApproveStudentRegistration(userId);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        BadRequestObjectResult? badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult?.Value, Is.EqualTo($"Student with user ID {userId} is not in PendingApproval status."));
        _mockStudentRepository.Verify(r => r.UpdateStudentAsync(It.IsAny<Jahoot.Core.Models.Student>()), Times.Never);
    }

    [Test]
    public void ApproveStudentRegistration_RequiresLecturerAuthorization()
    {
        MethodInfo? methodInfo = typeof(ApproveStudentController).GetMethod(nameof(ApproveStudentController.ApproveStudentRegistration));
        object[]? attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        Assert.That(attributes, Is.Not.Null);

        AuthorizeAttribute? authorizeAttribute = attributes.OfType<AuthorizeAttribute>().FirstOrDefault(a => a.Policy == nameof(Role.Lecturer));
        Assert.That(authorizeAttribute, Is.Not.Null, "Method should have [Authorize(Policy = \"Lecturer\")]");
    }
}
