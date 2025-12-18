using Dapper;
using Jahoot.Core.Models;
using Jahoot.WebApi.Models.Responses;
using Jahoot.WebApi.Repositories;

namespace Jahoot.WebApi.Tests.Repositories;

public class SubjectRepositoryTests : RepositoryTestBase
{
    private SubjectRepository _repository;

    [SetUp]
    public new void Setup()
    {
        _repository = new SubjectRepository(Connection);
    }

    [Test]
    public async Task CreateSubjectAsync_ValidName_CreatesSubject()
    {
        const string subjectName = "Mathematics";

        await _repository.CreateSubjectAsync(subjectName);

        Subject? subject = await Connection.QuerySingleOrDefaultAsync<Subject>("SELECT * FROM Subject WHERE name = @Name", new { Name = subjectName });
        Assert.That(subject, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(subject.Name, Is.EqualTo(subjectName));
            Assert.That(subject.IsActive, Is.True);
        }
    }

    [Test]
    public async Task GetAllSubjectsAsync_ReturnsAllSubjects()
    {
        await Connection.ExecuteAsync("INSERT INTO Subject (name, is_active) VALUES ('Maths', TRUE)");
        await Connection.ExecuteAsync("INSERT INTO Subject (name, is_active) VALUES ('Science', FALSE)");

        Subject[] subjects = (await _repository.GetAllSubjectsAsync()).ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(subjects, Has.Length.EqualTo(2));
            Assert.That(subjects.Select(s => s.Name), Is.EquivalentTo(["Maths", "Science"]));
        }
    }

    [Test]
    public async Task GetAllSubjectsAsync_WithIsActiveTrue_ReturnsOnlyActiveSubjects()
    {
        await Connection.ExecuteAsync("INSERT INTO Subject (name, is_active) VALUES ('ActiveSubject1', TRUE)");
        await Connection.ExecuteAsync("INSERT INTO Subject (name, is_active) VALUES ('InactiveSubject1', FALSE)");
        await Connection.ExecuteAsync("INSERT INTO Subject (name, is_active) VALUES ('ActiveSubject2', TRUE)");

        Subject[] subjects = (await _repository.GetAllSubjectsAsync(true)).ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(subjects, Has.Length.EqualTo(2));
            Assert.That(subjects.Select(s => s.Name), Is.EquivalentTo(["ActiveSubject1", "ActiveSubject2"]));
        }
    }

    [Test]
    public async Task GetAllSubjectsAsync_WithIsActiveFalse_ReturnsOnlyInactiveSubjects()
    {
        await Connection.ExecuteAsync("INSERT INTO Subject (name, is_active) VALUES ('ActiveSubject1', TRUE)");
        await Connection.ExecuteAsync("INSERT INTO Subject (name, is_active) VALUES ('InactiveSubject1', FALSE)");
        await Connection.ExecuteAsync("INSERT INTO Subject (name, is_active) VALUES ('InactiveSubject2', FALSE)");

        Subject[] subjects = (await _repository.GetAllSubjectsAsync(false)).ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(subjects, Has.Length.EqualTo(2));
            Assert.That(subjects.Select(s => s.Name), Is.EquivalentTo(["InactiveSubject1", "InactiveSubject2"]));
        }
    }

    [Test]
    public async Task GetSubjectByIdAsync_SubjectExists_ReturnsSubject()
    {
        const string subjectName = "History";
        await Connection.ExecuteAsync("INSERT INTO Subject (name) VALUES (@Name)", new { Name = subjectName });
        int subjectId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        Subject? result = await _repository.GetSubjectByIdAsync(subjectId);

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.SubjectId, Is.EqualTo(subjectId));
            Assert.That(result.Name, Is.EqualTo(subjectName));
        }
    }

    [Test]
    public async Task GetSubjectByIdAsync_SubjectDoesNotExist_ReturnsNull()
    {
        Subject? result = await _repository.GetSubjectByIdAsync(999);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetSubjectByNameAsync_SubjectExists_ReturnsSubject()
    {
        const string subjectName = "Physics";
        await Connection.ExecuteAsync("INSERT INTO Subject (name) VALUES (@Name)", new { Name = subjectName });

        Subject? result = await _repository.GetSubjectByNameAsync(subjectName);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo(subjectName));
    }

    [Test]
    public async Task GetSubjectByNameAsync_SubjectDoesNotExist_ReturnsNull()
    {
        Subject? result = await _repository.GetSubjectByNameAsync("NonExistent");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateSubjectAsync_ValidSubject_UpdatesDatabase()
    {
        const string initialName = "Biology";
        await Connection.ExecuteAsync("INSERT INTO Subject (name, is_active) VALUES (@Name, TRUE)", new { Name = initialName });
        int subjectId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        Subject subjectToUpdate = new()
        {
            SubjectId = subjectId,
            Name = "Advanced Biology",
            IsActive = false
        };

        await _repository.UpdateSubjectAsync(subjectToUpdate);

        Subject? updatedSubject = await Connection.QuerySingleOrDefaultAsync<Subject>("SELECT * FROM Subject WHERE subject_id = @Id", new { Id = subjectId });

        Assert.That(updatedSubject, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(updatedSubject.Name, Is.EqualTo("Advanced Biology"));
            Assert.That(updatedSubject.IsActive, Is.False);
        }
    }
    [Test]
    public async Task GetLeaderboardForSubjectAsync_ReturnsTop5Students()
    {
        // Create Subject
        await Connection.ExecuteAsync("INSERT INTO Subject (name) VALUES ('LeaderboardSubject')");
        int subjectId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        // Create Test
        await Connection.ExecuteAsync("INSERT INTO Test (subject_id, name, number_of_questions) VALUES (@SubjectId, 'Test1', 10)", new { SubjectId = subjectId });
        int testId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        // Create Students and Results
        // Create 6 students to verify limit 5
        for (int i = 1; i <= 6; i++)
        {
            await Connection.ExecuteAsync("INSERT INTO User (name, email, password_hash) VALUES (@Name, @Email, 'hash')",
                new { Name = $"Student{i}", Email = $"s{i}@test.com" });
            int userId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

            await Connection.ExecuteAsync("INSERT INTO Student (user_id) VALUES (@UserId)", new { UserId = userId });
            int studentId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

            // Score: Student1=10, Student2=20, ..., Student6=60
            int score = i * 10;
            await Connection.ExecuteAsync(
                "INSERT INTO TestResult (test_id, student_id, completion_date, questions_correct, score) VALUES (@TestId, @StudentId, NOW(), 0, @Score)",
                new { TestId = testId, StudentId = studentId, Score = score });
        }

        List<LeaderboardEntry> leaderboard = (await _repository.GetLeaderboardForSubjectAsync(subjectId)).ToList();

        Assert.That(leaderboard, Has.Count.EqualTo(5));
        using (Assert.EnterMultipleScope())
        {

            // Student6 should be first (Score 60)
            Assert.That(leaderboard[0].StudentName, Is.EqualTo("Student6"));
            Assert.That(leaderboard[0].TotalScore, Is.EqualTo(60));
            Assert.That(leaderboard[0].Rank, Is.EqualTo(1));

            // Student2 should be last in top 5 (Score 20)
            Assert.That(leaderboard[4].StudentName, Is.EqualTo("Student2"));
            Assert.That(leaderboard[4].TotalScore, Is.EqualTo(20));
            Assert.That(leaderboard[4].Rank, Is.EqualTo(5));
        }
    }

    [Test]
    public async Task GetLeaderboardForSubjectAsync_ExcludesDisabledUsers()
    {
        // Create Subject
        await Connection.ExecuteAsync("INSERT INTO Subject (name) VALUES ('LeaderboardSubject2')");
        int subjectId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        // Create Test
        await Connection.ExecuteAsync("INSERT INTO Test (subject_id, name, number_of_questions) VALUES (@SubjectId, 'Test1', 10)", new { SubjectId = subjectId });
        int testId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        // Create Active Student
        await Connection.ExecuteAsync("INSERT INTO User (name, email, password_hash, is_disabled) VALUES ('ActiveUser', 'active@test.com', 'hash', FALSE)");
        int activeUserId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");
        await Connection.ExecuteAsync("INSERT INTO Student (user_id) VALUES (@UserId)", new { UserId = activeUserId });
        int activeStudentId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        await Connection.ExecuteAsync("INSERT INTO TestResult (test_id, student_id, completion_date, questions_correct, score) VALUES (@TestId, @StudentId, NOW(), 0, 100)",
            new { TestId = testId, StudentId = activeStudentId });

        // Create Disabled Student
        await Connection.ExecuteAsync("INSERT INTO User (name, email, password_hash, is_disabled) VALUES ('DisabledUser', 'disabled@test.com', 'hash', TRUE)");
        int disabledUserId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");
        await Connection.ExecuteAsync("INSERT INTO Student (user_id) VALUES (@UserId)", new { UserId = disabledUserId });
        int disabledStudentId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        await Connection.ExecuteAsync("INSERT INTO TestResult (test_id, student_id, completion_date, questions_correct, score) VALUES (@TestId, @StudentId, NOW(), 0, 200)",
            new { TestId = testId, StudentId = disabledStudentId });

        List<LeaderboardEntry> leaderboard = (await _repository.GetLeaderboardForSubjectAsync(subjectId)).ToList();

        Assert.That(leaderboard, Has.Count.EqualTo(1));
        Assert.That(leaderboard[0].StudentName, Is.EqualTo("ActiveUser"));
    }
}
