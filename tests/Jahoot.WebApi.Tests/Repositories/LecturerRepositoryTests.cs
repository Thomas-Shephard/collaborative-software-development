using Dapper;
using Jahoot.Core.Models;
using Jahoot.WebApi.Repositories;
using Moq;

namespace Jahoot.WebApi.Tests.Repositories;

public class LecturerRepositoryTests : RepositoryTestBase
{
    private LecturerRepository _repository;
    private Mock<IUserRepository> _mockUserRepository;

    [SetUp]
    public new void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _repository = new LecturerRepository(Connection, _mockUserRepository.Object);
    }

    [Test]
    public async Task CreateLecturerAsync_FullDetails_CreatesUserAndLecturer()
    {
        const string name = "Dr. Test";
        const string email = "test@example.com";
        const string passwordHash = "hash";
        const bool isAdmin = true;

        await _repository.CreateLecturerAsync(name, email, passwordHash, isAdmin);

        User user = await Connection.QuerySingleAsync<User>("SELECT * FROM User WHERE email = @Email", new { Email = email });
        Assert.That(user, Is.Not.Null);

        dynamic lecturer = await Connection.QuerySingleAsync<dynamic>("SELECT * FROM Lecturer WHERE user_id = @UserId", new { user.UserId });
        Assert.That(lecturer, Is.Not.Null);
        Assert.That(lecturer.is_admin, Is.EqualTo(isAdmin));
    }

    [Test]
    public async Task CreateLecturerAsync_MixedCaseEmail_CreatesUserWithLowercaseEmail()
    {
        const string mixedCaseEmail = "Dr.MiXeD@ExAmPlE.cOm";
        await _repository.CreateLecturerAsync("Dr. Mixed", mixedCaseEmail, "hash", false);

        User user = await Connection.QuerySingleAsync<User>("SELECT * FROM User WHERE email = 'dr.mixed@example.com'");
        Assert.That(user.Email, Is.EqualTo("dr.mixed@example.com"));
    }

    [Test]
    public async Task CreateLecturerAsync_ExistingUserId_CreatesLecturer()
    {
        await Connection.ExecuteAsync("INSERT INTO User (name, email, password_hash) VALUES ('User', 'user@test.com', 'hash')");
        int userId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        await _repository.CreateLecturerAsync(userId, false);

        dynamic lecturer = await Connection.QuerySingleAsync<dynamic>("SELECT * FROM Lecturer WHERE user_id = @UserId", new { UserId = userId });
        Assert.That(lecturer, Is.Not.Null);
        Assert.That(lecturer.is_admin, Is.False);
    }

    [Test]
    public async Task GetLecturerByUserIdAsync_LecturerExists_ReturnsLecturer()
    {
        await _repository.CreateLecturerAsync("Dr. Test", "test@example.com", "hash", true);
        User user = await Connection.QuerySingleAsync<User>("SELECT * FROM User WHERE email = 'test@example.com'");

        _mockUserRepository.Setup(x => x.GetRolesByUserIdAsync(user.UserId))
            .ReturnsAsync([Role.Lecturer, Role.Admin]);

        Lecturer? result = await _repository.GetLecturerByUserIdAsync(user.UserId);

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.UserId, Is.EqualTo(user.UserId));
            Assert.That(result.Name, Is.EqualTo("Dr. Test"));
            Assert.That(result.IsAdmin, Is.True);
            Assert.That(result.Roles, Contains.Item(Role.Lecturer));
            Assert.That(result.Roles, Contains.Item(Role.Admin));
        }
    }

    [Test]
    public async Task GetLecturerByUserIdAsync_LecturerAndStudent_ReturnsBothRoles()
    {
        await _repository.CreateLecturerAsync("Dr. Multi", "multi@example.com", "hash", false);
        User user = await Connection.QuerySingleAsync<User>("SELECT * FROM User WHERE email = 'multi@example.com'");
        await Connection.ExecuteAsync("INSERT INTO Student (user_id) VALUES (@UserId)", new { user.UserId });

        _mockUserRepository.Setup(x => x.GetRolesByUserIdAsync(user.UserId))
            .ReturnsAsync([Role.Lecturer, Role.Student]);

        Lecturer? result = await _repository.GetLecturerByUserIdAsync(user.UserId);

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsAdmin, Is.False);
            Assert.That(result.Roles, Contains.Item(Role.Lecturer));
            Assert.That(result.Roles, Contains.Item(Role.Student));
            Assert.That(result.Roles, Does.Not.Contain(Role.Admin));
        }
    }

    [Test]
    public async Task GetLecturerByUserIdAsync_NotALecturer_ReturnsNull()
    {
        await Connection.ExecuteAsync("INSERT INTO User (name, email, password_hash) VALUES ('User', 'user@test.com', 'hash')");
        int userId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        Lecturer? result = await _repository.GetLecturerByUserIdAsync(userId);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetLecturersAsync_ReturnsAllLecturers()
    {
        await _repository.CreateLecturerAsync("L1", "l1@test.com", "hash", true);
        await _repository.CreateLecturerAsync("L2", "l2@test.com", "hash", false);

        _mockUserRepository.Setup(x => x.GetRolesByUserIdsAsync(It.IsAny<IEnumerable<int>>(), null))
            .ReturnsAsync([]);

        IEnumerable<Lecturer> result = await _repository.GetLecturersAsync();

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task UpdateLecturerAsync_UpdatesIsAdmin()
    {
        await _repository.CreateLecturerAsync("L1", "l1@test.com", "hash", true);
        User user = await Connection.QuerySingleAsync<User>("SELECT * FROM User WHERE email = 'l1@test.com'");

        _mockUserRepository.Setup(x => x.GetRolesByUserIdAsync(user.UserId))
            .ReturnsAsync([Role.Lecturer, Role.Admin]);

        Lecturer? lecturer = await _repository.GetLecturerByUserIdAsync(user.UserId);

        Assert.That(lecturer, Is.Not.Null);
        lecturer.IsAdmin = false;

        await _repository.UpdateLecturerAsync(lecturer);

        dynamic dbLecturer = await Connection.QuerySingleAsync<dynamic>("SELECT * FROM Lecturer WHERE user_id = @UserId", new { user.UserId });
        Assert.That(dbLecturer.is_admin, Is.False);
    }
}
