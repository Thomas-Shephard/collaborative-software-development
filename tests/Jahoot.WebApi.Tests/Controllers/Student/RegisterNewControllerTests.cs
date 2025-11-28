using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Controllers.Student;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Student;

public class RegisterNewControllerTests
{
    private const string UserEmail = "test@example.com";
    private const string UserName = "Test User";
    private const string UserPassword = "password123";

    private Mock<IStudentRepository> _studentRepositoryMock;
    private Mock<IUserRepository> _userRepositoryMock;
    private RegisterNewController _controller;

    [SetUp]
    public void Setup()
    {
        _studentRepositoryMock = new Mock<IStudentRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _controller = new RegisterNewController(_studentRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Test]
    public async Task RegisterStudent_EmailExists_ReturnsConflict()
    {
        StudentRegistrationRequestModel request = new()
        {
            Email = UserEmail,
            Name = UserName,
            Password = UserPassword
        };
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(UserEmail))
            .ReturnsAsync(new User
            {
                Email = UserEmail,
                Name = "Existing",
                PasswordHash = "hash",
                Roles = new List<Role>()
            });

        IActionResult result = await _controller.RegisterNewStudent(request);

        Assert.That(result, Is.TypeOf<ConflictObjectResult>());
        ConflictObjectResult conflictResult = (ConflictObjectResult)result;
        Assert.That(conflictResult.Value, Is.EqualTo("A user with this email address already exists."));
        _studentRepositoryMock.Verify(repo => repo.CreateStudentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task RegisterStudent_EmailDoesNotExist_ReturnsCreated()
    {
        StudentRegistrationRequestModel request = new()
        {
            Email = UserEmail,
            Name = UserName,
            Password = UserPassword
        };
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(UserEmail))
            .ReturnsAsync((User?)null);

        IActionResult result = await _controller.RegisterNewStudent(request);

        Assert.That(result, Is.TypeOf<StatusCodeResult>());
        StatusCodeResult statusCodeResult = (StatusCodeResult)result;
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(201));

        _studentRepositoryMock.Verify(repo => repo.CreateStudentAsync(UserName, UserEmail, It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task RegisterStudent_InvalidModel_ReturnsBadRequest()
    {
         _controller.ModelState.AddModelError("Email", "Invalid email");
         StudentRegistrationRequestModel request = new() { Email = "bad", Name = "Name", Password = "pass" };

         IActionResult result = await _controller.RegisterNewStudent(request);

         Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
         _studentRepositoryMock.Verify(repo => repo.CreateStudentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
