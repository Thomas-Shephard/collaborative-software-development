using System.Reflection;
using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Controllers.Lecturer;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Lecturer;

public class RegisterNewLecturerControllerTests
{
    private Mock<ILecturerRepository> _lecturerRepositoryMock;
    private Mock<IUserRepository> _userRepositoryMock;
    private RegisterNewLecturerController _controller;

    [SetUp]
    public void Setup()
    {
        _lecturerRepositoryMock = new Mock<ILecturerRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _controller = new RegisterNewLecturerController(_lecturerRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Test]
    public async Task RegisterLecturer_EmailExists_ReturnsConflict()
    {
        CreateLecturerRequestModel request = new()
        {
            Email = "existing@example.com",
            Name = "Dr. Existing",
            Password = "password",
            IsAdmin = false
        };
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(request.Email)).ReturnsAsync(new User { Email = request.Email, Name = "Existing", PasswordHash = "hash", Roles = [] });

        IActionResult result = await _controller.RegisterLecturer(request);

        Assert.That(result, Is.TypeOf<ConflictObjectResult>());
        ConflictObjectResult conflictResult = (ConflictObjectResult)result;
        Assert.That(conflictResult.Value, Is.EqualTo("A user with this email address already exists."));
    }

    [Test]
    public async Task RegisterLecturer_ValidRequest_ReturnsCreated()
    {
        CreateLecturerRequestModel request = new()
        {
            Email = "new@example.com",
            Name = "Dr. New",
            Password = "password",
            IsAdmin = true
        };
        _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(request.Email)).ReturnsAsync((User?)null);

        IActionResult result = await _controller.RegisterLecturer(request);

        Assert.That(result, Is.TypeOf<CreatedResult>());
        _lecturerRepositoryMock.Verify(repo => repo.CreateLecturerAsync(request.Name, request.Email, It.IsAny<string>(), request.IsAdmin), Times.Once);
    }

    [Test]
    public async Task RegisterLecturer_InvalidModel_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("Email", "Invalid email");
        CreateLecturerRequestModel request = new() { Email = "bad", Name = "Name", Password = "pass", IsAdmin = false };

        IActionResult result = await _controller.RegisterLecturer(request);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        _lecturerRepositoryMock.Verify(repo => repo.CreateLecturerAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void RegisterLecturer_HasAdminAuthorization()
    {
        MethodInfo? methodInfo = typeof(RegisterNewLecturerController).GetMethod(nameof(RegisterNewLecturerController.RegisterLecturer));
        object[]? attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        Assert.That(attributes, Is.Not.Null);
        AuthorizeAttribute? authorizeAttribute = attributes.OfType<AuthorizeAttribute>().FirstOrDefault(a => a.Policy == nameof(Role.Admin));
        Assert.That(authorizeAttribute, Is.Not.Null, "Method should have [Authorize(Policy = \"Admin\")]");
    }
}
