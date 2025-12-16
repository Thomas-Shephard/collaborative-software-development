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

    private async Task<(int studentId, int userId)> CreateStudentInDb(string name = UserName, string email = UserEmail, string passwordHash = UserPasswordHash, StudentAccountStatus status = StudentAccountStatus.PendingApproval)
    {
        await Connection.ExecuteAsync("INSERT INTO User (name, email, password_hash) VALUES (@Name, @Email, @Hash)", new { Name = name, Email = email, Hash = passwordHash });
        int userId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");
        string statusString = status switch
        {
            StudentAccountStatus.PendingApproval => "pending_approval",
            StudentAccountStatus.Active => "active",
            StudentAccountStatus.Disabled => "disabled",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
        await Connection.ExecuteAsync("INSERT INTO Student (user_id, account_status) VALUES (@UserId, @AccountStatus)", new { UserId = userId, AccountStatus = statusString });
        int studentId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");
        return (studentId, userId);
    }

    private async Task<int> CreateSubjectInDb(string subjectName)
    {
        await Connection.ExecuteAsync("INSERT INTO Subject (name) VALUES (@Name)", new { Name = subjectName });
        return await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");
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
    public async Task GetStudentByUserIdAsync_StudentExists_ReturnsStudentWithRolesAndNoSubjects()
    {
        (int studentId, int userId) = await CreateStudentInDb();

        Student? result = await _repository.GetStudentByUserIdAsync(userId);

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.UserId, Is.EqualTo(userId));
            Assert.That(result.Name, Is.EqualTo(UserName));
            Assert.That(result.Email, Is.EqualTo(UserEmail));
            Assert.That(result.Roles, Contains.Item(Role.Student));
            Assert.That(result.AccountStatus, Is.EqualTo(StudentAccountStatus.PendingApproval));
            Assert.That(result.StudentId, Is.EqualTo(studentId));
            Assert.That(result.Subjects, Is.Empty);
        }
    }

    [Test]
    public async Task GetStudentByUserIdAsync_StudentExistsWithSubjects_ReturnsStudentWithSubjects()
    {
        (int studentId, int userId) = await CreateStudentInDb();
        int subject1Id = await CreateSubjectInDb("Math");
        int subject2Id = await CreateSubjectInDb("Science");

        await Connection.ExecuteAsync("INSERT INTO StudentSubject (student_id, subject_id) VALUES (@StudentId, @SubjectId)", new { StudentId = studentId, SubjectId = subject1Id });
        await Connection.ExecuteAsync("INSERT INTO StudentSubject (student_id, subject_id) VALUES (@StudentId, @SubjectId)", new { StudentId = studentId, SubjectId = subject2Id });

        Student? result = await _repository.GetStudentByUserIdAsync(userId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Subjects, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Subjects.Any(s => s.SubjectId == subject1Id && s.Name == "Math"), Is.True);
            Assert.That(result.Subjects.Any(s => s.SubjectId == subject2Id && s.Name == "Science"), Is.True);
        }
    }

    [Test]
    public async Task GetStudentByUserIdAsync_StudentAndLecturer_ReturnsBothRoles()
    {
        (int studentId, int userId) = await CreateStudentInDb(status: StudentAccountStatus.Active);
        await Connection.ExecuteAsync("INSERT INTO Lecturer (user_id, is_admin) VALUES (@UserId, FALSE)", new { UserId = userId });

        Student? result = await _repository.GetStudentByUserIdAsync(userId);

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Roles, Contains.Item(Role.Student));
            Assert.That(result.Roles, Contains.Item(Role.Lecturer));
            Assert.That(result.AccountStatus, Is.EqualTo(StudentAccountStatus.Active));
            Assert.That(result.StudentId, Is.EqualTo(studentId));
        }
    }

    [Test]
    public async Task GetStudentByUserIdAsync_StudentAndAdmin_ReturnsThreeRoles()
    {
        (int studentId, int userId) = await CreateStudentInDb(status: StudentAccountStatus.Disabled);
        await Connection.ExecuteAsync("INSERT INTO Lecturer (user_id, is_admin) VALUES (@UserId, TRUE)", new { UserId = userId });

        Student? result = await _repository.GetStudentByUserIdAsync(userId);

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Roles, Contains.Item(Role.Student));
            Assert.That(result.Roles, Contains.Item(Role.Lecturer));
            Assert.That(result.Roles, Contains.Item(Role.Admin));
            Assert.That(result.AccountStatus, Is.EqualTo(StudentAccountStatus.Disabled));
            Assert.That(result.StudentId, Is.EqualTo(studentId));
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
    public async Task GetStudentsByStatusAsync_ReturnsMatchingStudentsWithSubjects()
    {
        (int pendingStudentId, int pendingUserId) = await CreateStudentInDb("Pending", "pending@test.com", "hash");
        (int activeStudentId, int activeUserId) = await CreateStudentInDb("Active", "active@test.com", "hash", StudentAccountStatus.Active);

        int mathSubjectId = await CreateSubjectInDb("Math");
        int physicsSubjectId = await CreateSubjectInDb("Physics");

        await Connection.ExecuteAsync("INSERT INTO StudentSubject (student_id, subject_id) VALUES (@StudentId, @SubjectId)", new { StudentId = pendingStudentId, SubjectId = mathSubjectId });
        await Connection.ExecuteAsync("INSERT INTO StudentSubject (student_id, subject_id) VALUES (@StudentId, @SubjectId)", new { StudentId = activeStudentId, SubjectId = mathSubjectId });
        await Connection.ExecuteAsync("INSERT INTO StudentSubject (student_id, subject_id) VALUES (@StudentId, @SubjectId)", new { StudentId = activeStudentId, SubjectId = physicsSubjectId });


        Student[] result = (await _repository.GetStudentsByStatusAsync(StudentAccountStatus.PendingApproval))
                                             .ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result[0].UserId, Is.EqualTo(pendingUserId));
            Assert.That(result[0].AccountStatus, Is.EqualTo(StudentAccountStatus.PendingApproval));
            Assert.That(result[0].Subjects, Has.Count.EqualTo(1));
            Assert.That(result[0].Subjects.Any(s => s.SubjectId == mathSubjectId && s.Name == "Math"), Is.True);
        }

        result = (await _repository.GetStudentsByStatusAsync(StudentAccountStatus.Active))
                                             .ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result[0].UserId, Is.EqualTo(activeUserId));
            Assert.That(result[0].AccountStatus, Is.EqualTo(StudentAccountStatus.Active));
            Assert.That(result[0].Subjects, Has.Count.EqualTo(2));
            Assert.That(result[0].Subjects.Any(s => s.SubjectId == mathSubjectId && s.Name == "Math"), Is.True);
            Assert.That(result[0].Subjects.Any(s => s.SubjectId == physicsSubjectId && s.Name == "Physics"), Is.True);
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

    [Test]
    public async Task UpdateStudentAsync_UpdatesAccountStatusAndAddsSubjects()
    {
        (int studentId, int userId) = await CreateStudentInDb();
        int subject1Id = await CreateSubjectInDb("Math");
        int subject2Id = await CreateSubjectInDb("Science");

        Student? student = await _repository.GetStudentByUserIdAsync(userId);
        Assert.That(student, Is.Not.Null);

        student.AccountStatus = StudentAccountStatus.Active;
        student.Subjects = new List<Subject>
        {
            new() { SubjectId = subject1Id, Name = "Math" },
            new() { SubjectId = subject2Id, Name = "Science" }
        };

        await _repository.UpdateStudentAsync(student);

        dynamic dbStudent = await Connection.QuerySingleAsync<dynamic>("SELECT * FROM Student WHERE user_id = @UserId", new { userId });
        Assert.That(dbStudent.account_status, Is.EqualTo("active"));

        dynamic[] studentSubjects = (await Connection.QueryAsync<dynamic>("SELECT * FROM StudentSubject WHERE student_id = @StudentId", new { StudentId = studentId })).ToArray();
        Assert.That(studentSubjects, Has.Length.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(studentSubjects.Any(ss => ss.subject_id == subject1Id), Is.True);
            Assert.That(studentSubjects.Any(ss => ss.subject_id == subject2Id), Is.True);
        }
    }

    [Test]
    public async Task UpdateStudentAsync_UpdatesAccountStatusAndRemovesSubjects()
    {
        (int studentId, int userId) = await CreateStudentInDb();
        int subject1Id = await CreateSubjectInDb("Math");
        int subject2Id = await CreateSubjectInDb("Science");

        await Connection.ExecuteAsync("INSERT INTO StudentSubject (student_id, subject_id) VALUES (@StudentId, @SubjectId)", new { StudentId = studentId, SubjectId = subject1Id });
        await Connection.ExecuteAsync("INSERT INTO StudentSubject (student_id, subject_id) VALUES (@StudentId, @SubjectId)", new { StudentId = studentId, SubjectId = subject2Id });

        Student? student = await _repository.GetStudentByUserIdAsync(userId);
        Assert.That(student, Is.Not.Null);

        student.AccountStatus = StudentAccountStatus.Active;
        student.Subjects = new List<Subject>
        {
            new() { SubjectId = subject1Id, Name = "Math" }
        };

        await _repository.UpdateStudentAsync(student);

        dynamic dbStudent = await Connection.QuerySingleAsync<dynamic>("SELECT * FROM Student WHERE user_id = @UserId", new { userId });
        Assert.That(dbStudent.account_status, Is.EqualTo("active"));

        dynamic[] studentSubjects = (await Connection.QueryAsync<dynamic>("SELECT * FROM StudentSubject WHERE student_id = @StudentId", new { StudentId = studentId })).ToArray();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(studentSubjects.Any(ss => ss.subject_id == subject1Id), Is.True);
            Assert.That(studentSubjects.Any(ss => ss.subject_id == subject2Id), Is.False);
        }
    }

    [Test]
    public async Task UpdateStudentAsync_UpdatesAccountStatusAndNoSubjects()
    {
        (int studentId, int userId) = await CreateStudentInDb();
        int subject1Id = await CreateSubjectInDb("Math");

        await Connection.ExecuteAsync("INSERT INTO StudentSubject (student_id, subject_id) VALUES (@StudentId, @SubjectId)", new { StudentId = studentId, SubjectId = subject1Id });

        Student? student = await _repository.GetStudentByUserIdAsync(userId);
        Assert.That(student, Is.Not.Null);

        student.AccountStatus = StudentAccountStatus.Active;
        student.Subjects = new List<Subject>();

        await _repository.UpdateStudentAsync(student);

        dynamic dbStudent = await Connection.QuerySingleAsync<dynamic>("SELECT * FROM Student WHERE user_id = @UserId", new { userId });
        Assert.That(dbStudent.account_status, Is.EqualTo("active"));

        IEnumerable<dynamic> studentSubjects = await Connection.QueryAsync<dynamic>("SELECT * FROM StudentSubject WHERE student_id = @StudentId", new { StudentId = studentId });
        Assert.That(studentSubjects, Is.Empty);
    }
}
