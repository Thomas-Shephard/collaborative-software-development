using System.Reflection;
using Jahoot.Core.Models;
using Jahoot.WebApi.Controllers.Lecturer;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Jahoot.WebApi.Tests.Controllers.Lecturer;

public class ListLecturersControllerTests
{
    private Mock<ILecturerRepository> _lecturerRepositoryMock;
    private ListLecturersController _controller;

    [SetUp]
    public void Setup()
    {
        _lecturerRepositoryMock = new Mock<ILecturerRepository>();
        _controller = new ListLecturersController(_lecturerRepositoryMock.Object);
    }

    [Test]
    public async Task GetLecturers_ReturnsOkWithLecturers()
    {
        List<Core.Models.Lecturer> lecturers =
        [
            new() { LecturerId = 1, Name = "Lecturer 1", Email = "l1@example.com", PasswordHash = "hash", Roles = [] },
            new() { LecturerId = 2, Name = "Lecturer 2", Email = "l2@example.com", PasswordHash = "hash", Roles = [] }
        ];
        _lecturerRepositoryMock.Setup(repo => repo.GetLecturersAsync()).ReturnsAsync(lecturers);

        IActionResult result = await _controller.GetLecturers();

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        OkObjectResult okResult = (OkObjectResult)result;
        Core.Models.Lecturer[]? returnedLecturers = (okResult.Value as IEnumerable<Core.Models.Lecturer>)?.ToArray();

        Assert.That(returnedLecturers, Is.Not.Null);
        Assert.That(returnedLecturers, Has.Length.EqualTo(2));
    }

    [Test]
    public void GetLecturers_HasAdminAuthorization()
    {
        MethodInfo? methodInfo = typeof(ListLecturersController).GetMethod(nameof(ListLecturersController.GetLecturers));
        object[]? attributes = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), false);

        Assert.That(attributes, Is.Not.Null);
        AuthorizeAttribute? authorizeAttribute = attributes.OfType<AuthorizeAttribute>().FirstOrDefault(a => a.Policy == nameof(Role.Admin));
        Assert.That(authorizeAttribute, Is.Not.Null, "Method should have [Authorize(Policy = \"Admin\")]");
    }
}
