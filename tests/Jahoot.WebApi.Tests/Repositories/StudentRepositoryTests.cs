using Dapper;
using Jahoot.Core.Models;
using Jahoot.WebApi.Repositories;

namespace Jahoot.WebApi.Tests.Repositories;

public class StudentRepositoryTests : RepositoryTestBase
{
    private const string UserEmail = "test@example.com";
    private const string UserName = "Test User";
    private const string UserPasswordHash = "hashed_password";

    private StudentRepository _repository;

    [SetUp]
    public new async Task Setup()
    {
        await base.Setup();
        _repository = new StudentRepository(Connection);
    }

    [Test]
    public async Task CreateStudentAsync_FullDetails_CreatesUserAndStudent()
    {
        await _repository.CreateStudentAsync(UserName, UserEmail, UserPasswordHash);

        User user = await Connection.QuerySingleAsync<User>("SELECT * FROM User WHERE email = @Email", new { Email = UserEmail });
        Assert.That(user, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(user.Name, Is.EqualTo(UserName));
            Assert.That(user.PasswordHash, Is.EqualTo(UserPasswordHash));
        }

        dynamic student = await Connection.QuerySingleAsync<dynamic>("SELECT * FROM Student WHERE user_id = @UserId", new { user.UserId });
        Assert.That(student, Is.Not.Null);
        Assert.That(student.account_status, Is.EqualTo("pending_approval"));
    }

    [Test]
    public async Task CreateStudentAsync_ClosedConnection_OpensConnectionAndCreatesStudent()
    {
        await Connection.CloseAsync();

        await _repository.CreateStudentAsync(UserName, UserEmail, UserPasswordHash);

        User user = await Connection.QuerySingleAsync<User>("SELECT * FROM User WHERE email = @Email", new { Email = UserEmail });
        Assert.That(user, Is.Not.Null);
    }

    [Test]
    public async Task CreateStudentAsync_ExistingUserId_CreatesStudent()
    {
        await Connection.ExecuteAsync("INSERT INTO User (name, email, password_hash) VALUES (@Name, @Email, @Hash)", new { Name = UserName, Email = UserEmail, Hash = UserPasswordHash });
        int userId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        await _repository.CreateStudentAsync(userId);

        dynamic student = await Connection.QuerySingleAsync<dynamic>("SELECT * FROM Student WHERE user_id = @UserId", new { UserId = userId });
        Assert.That(student, Is.Not.Null);
        Assert.That(student.account_status, Is.EqualTo("pending_approval"));
    }

    [Test]
    public async Task GetStudentByUserIdAsync_StudentExists_ReturnsStudentWithRoles()
    {
        await _repository.CreateStudentAsync(UserName, UserEmail, UserPasswordHash);
        User user = await Connection.QuerySingleAsync<User>("SELECT * FROM User WHERE email = @Email", new { Email = UserEmail });
        dynamic dbStudent = await Connection.QuerySingleAsync<dynamic>("SELECT student_id FROM Student WHERE user_id = @UserId", new { user.UserId });

        Student? result = await _repository.GetStudentByUserIdAsync(user.UserId);

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.UserId, Is.EqualTo(user.UserId));
            Assert.That(result.Name, Is.EqualTo(UserName));
            Assert.That(result.Email, Is.EqualTo(UserEmail));
            Assert.That(result.Roles, Contains.Item(Role.Student));
            Assert.That(result.AccountStatus, Is.EqualTo(StudentAccountStatus.PendingApproval));
            Assert.That(result.StudentId, Is.EqualTo(dbStudent.student_id));
        }
    }

    [Test]
    public async Task GetStudentByUserIdAsync_StudentAndLecturer_ReturnsBothRoles()
    {
        await Connection.ExecuteAsync("INSERT INTO User (name, email, password_hash) VALUES (@Name, @Email, @Hash)", new { Name = UserName, Email = UserEmail, Hash = UserPasswordHash });
        int userId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        await Connection.ExecuteAsync("INSERT INTO Student (user_id, account_status) VALUES (@UserId, 'active')", new { UserId = userId });
        await Connection.ExecuteAsync("INSERT INTO Lecturer (user_id, is_admin) VALUES (@UserId, FALSE)", new { UserId = userId });
        dynamic dbStudent = await Connection.QuerySingleAsync<dynamic>("SELECT student_id FROM Student WHERE user_id = @UserId", new { UserId = userId });

        Student? result = await _repository.GetStudentByUserIdAsync(userId);

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Roles, Contains.Item(Role.Student));
            Assert.That(result.Roles, Contains.Item(Role.Lecturer));
            Assert.That(result.AccountStatus, Is.EqualTo(StudentAccountStatus.Active));
            Assert.That(result.StudentId, Is.EqualTo(dbStudent.student_id));
        }
    }

    [Test]
    public async Task GetStudentByUserIdAsync_StudentAndAdmin_ReturnsThreeRoles()
    {
        await Connection.ExecuteAsync("INSERT INTO User (name, email, password_hash) VALUES (@Name, @Email, @Hash)", new { Name = UserName, Email = UserEmail, Hash = UserPasswordHash });
        int userId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        await Connection.ExecuteAsync("INSERT INTO Student (user_id, account_status) VALUES (@UserId, 'disabled')", new { UserId = userId });
        await Connection.ExecuteAsync("INSERT INTO Lecturer (user_id, is_admin) VALUES (@UserId, TRUE)", new { UserId = userId });
        dynamic dbStudent = await Connection.QuerySingleAsync<dynamic>("SELECT student_id FROM Student WHERE user_id = @UserId", new { UserId = userId });

        Student? result = await _repository.GetStudentByUserIdAsync(userId);

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Roles, Contains.Item(Role.Student));
            Assert.That(result.Roles, Contains.Item(Role.Lecturer));
            Assert.That(result.Roles, Contains.Item(Role.Admin));
            Assert.That(result.AccountStatus, Is.EqualTo(StudentAccountStatus.Disabled));
            Assert.That(result.StudentId, Is.EqualTo(dbStudent.student_id));
        }
    }

    [Test]
    public async Task GetStudentByUserIdAsync_NotAStudent_ReturnsNull()
    {
        await Connection.ExecuteAsync("INSERT INTO User (name, email, password_hash) VALUES (@Name, @Email, @Hash)", new { Name = UserName, Email = UserEmail, Hash = UserPasswordHash });
        int userId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        Student? result = await _repository.GetStudentByUserIdAsync(userId);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetStudentsByStatusAsync_ReturnsMatchingStudents()
    {
        await Connection.ExecuteAsync("INSERT INTO User (name, email, password_hash) VALUES ('Pending', 'pending@test.com', 'hash')");
        int pendingId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");
        await Connection.ExecuteAsync("INSERT INTO Student (user_id, account_status) VALUES (@UserId, 'pending_approval')", new { UserId = pendingId });

        await Connection.ExecuteAsync("INSERT INTO User (name, email, password_hash) VALUES ('Active', 'active@test.com', 'hash')");
        int activeId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");
        await Connection.ExecuteAsync("INSERT INTO Student (user_id, account_status) VALUES (@UserId, 'active')", new { UserId = activeId });

        Student[] result = (await _repository.GetStudentsByStatusAsync(StudentAccountStatus.PendingApproval))
                                             .ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result[0].UserId, Is.EqualTo(pendingId));
            Assert.That(result[0].AccountStatus, Is.EqualTo(StudentAccountStatus.PendingApproval));
        }
    }

    [Test]
    public async Task GetStudentByUserIdAsync_UnknownStatus_ThrowsException()
    {
        await Connection.ExecuteAsync("INSERT INTO User (name, email, password_hash) VALUES (@Name, @Email, @Hash)", new { Name = UserName, Email = UserEmail, Hash = UserPasswordHash });
        int userId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        try
        {
            // Alter table to allow invalid string and insert invalid status
            await Connection.ExecuteAsync("ALTER TABLE Student MODIFY COLUMN account_status VARCHAR(255)");
            await Connection.ExecuteAsync("INSERT INTO Student (user_id, account_status) VALUES (@UserId, 'invalid_status')", new { UserId = userId });

            InvalidOperationException? ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _repository.GetStudentByUserIdAsync(userId));
            Assert.That(ex.Message, Does.Contain("Unknown account status: invalid_status"));
        }
        finally
        {
            // Restore schema and delete the invalid row first to ensure clean schema reversion
            await Connection.ExecuteAsync("DELETE FROM Student WHERE user_id = @UserId", new { UserId = userId });
            await Connection.ExecuteAsync("ALTER TABLE Student MODIFY COLUMN account_status ENUM('pending_approval', 'active', 'disabled') NOT NULL DEFAULT 'pending_approval'");
        }
    }
}
