using Dapper;
using Jahoot.Core.Models;
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
        await Connection.ExecuteAsync("INSERT INTO Subject (name) VALUES ('Maths')");
        await Connection.ExecuteAsync("INSERT INTO Subject (name) VALUES ('Science')");

        Subject[] subjects = (await _repository.GetAllSubjectsAsync()).ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(subjects, Has.Length.EqualTo(2));
            Assert.That(subjects.Select(s => s.Name), Is.EquivalentTo(["Maths", "Science"]));
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
}
