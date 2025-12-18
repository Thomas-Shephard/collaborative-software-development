using Dapper;
using Jahoot.Core.Models;
using Jahoot.WebApi.Models.Responses;
using Jahoot.WebApi.Repositories;
using MySqlConnector;

namespace Jahoot.WebApi.Tests.Repositories;

public class TestRepositoryTests : RepositoryTestBase
{
    private TestRepository _repository;

    [SetUp]
    public new void Setup()
    {
        _repository = new TestRepository(Connection);
    }

    private async Task<int> CreateSubject(string name)
    {
        await Connection.ExecuteAsync("INSERT INTO Subject (name) VALUES (@Name)", new { Name = name });
        return await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");
    }

    [Test]
    public async Task CreateTestAsync_ValidTest_CreatesTestWithQuestionsAndOptions()
    {
        int subjectId = await CreateSubject("Maths");

        List<QuestionOption> options =
        [
            new() { OptionText = "4", IsCorrect = true },
            new() { OptionText = "5", IsCorrect = false }
        ];

        List<Question> questions =
        [
            new() { Text = "What is 2+2?", Options = options.AsReadOnly() }
        ];

        Test test = new()
        {
            SubjectId = subjectId,
            Name = "Addition Test",
            Questions = questions.AsReadOnly()
        };

        await _repository.CreateTestAsync(test);

        Test? dbTest = await Connection.QuerySingleOrDefaultAsync<Test>("SELECT * FROM Test WHERE name = @Name", new { test.Name });
        Assert.That(dbTest, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dbTest.SubjectId, Is.EqualTo(test.SubjectId));
            Assert.That(dbTest.Name, Is.EqualTo("Addition Test"));
        }

        int testId = dbTest.TestId;
        Question[] dbQuestions = (await Connection.QueryAsync<Question>("SELECT q.* FROM Question q JOIN TestQuestion tq ON q.question_id = tq.question_id WHERE tq.test_id = @TestId", new { TestId = testId })).ToArray();

        Assert.That(dbQuestions, Has.Length.EqualTo(1));
        Question dbQuestion = dbQuestions.First();
        Assert.That(dbQuestion.Text, Is.EqualTo("What is 2+2?"));

        QuestionOption[] dbOptions = (await Connection.QueryAsync<QuestionOption>("SELECT * FROM QuestionOption WHERE question_id = @QuestionId", new { dbQuestion.QuestionId })).ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dbOptions, Has.Length.EqualTo(2));
            Assert.That(dbOptions.Any(o => o is { OptionText: "4", IsCorrect: true }), Is.True);
            Assert.That(dbOptions.Any(o => o is { OptionText: "5", IsCorrect: false }), Is.True);
        }
    }

    [Test]
    public async Task GetAllTestsAsync_NoFilter_ReturnsAllTests()
    {
        int subjectId1 = await CreateSubject("Maths");
        int subjectId2 = await CreateSubject("Science");

        await Connection.ExecuteAsync("INSERT INTO Test (subject_id, name) VALUES (@SubId, 'Maths Test')", new { SubId = subjectId1 });
        await Connection.ExecuteAsync("INSERT INTO Test (subject_id, name) VALUES (@SubId, 'Science Test')", new { SubId = subjectId2 });

        IEnumerable<Test> tests = await _repository.GetAllTestsAsync();

        Assert.That(tests.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetAllTestsAsync_WithSubjectFilter_ReturnsMatchingTests()
    {
        int subjectId1 = await CreateSubject("Maths");
        int subjectId2 = await CreateSubject("Science");

        await Connection.ExecuteAsync("INSERT INTO Test (subject_id, name) VALUES (@SubId, 'Maths Test')", new { SubId = subjectId1 });
        await Connection.ExecuteAsync("INSERT INTO Test (subject_id, name) VALUES (@SubId, 'Science Test')", new { SubId = subjectId2 });

        Test[] tests = (await _repository.GetAllTestsAsync(subjectId1)).ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tests, Has.Length.EqualTo(1));
            Assert.That(tests[0].Name, Is.EqualTo("Maths Test"));
        }
    }

    [Test]
    public async Task GetTestByIdAsync_TestExists_ReturnsTestWithDetails()
    {
        int subjectId = await CreateSubject("Maths");
        await Connection.ExecuteAsync("INSERT INTO Test (subject_id, name) VALUES (@SubId, 'My Test')", new { SubId = subjectId });
        int testId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        await Connection.ExecuteAsync("INSERT INTO Question (text) VALUES ('Question 1')");
        int questionId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        await Connection.ExecuteAsync("INSERT INTO TestQuestion (test_id, question_id) VALUES (@TestId, @QuestionId)", new { TestId = testId, QuestionId = questionId });
        await Connection.ExecuteAsync("INSERT INTO QuestionOption (question_id, option_text, is_correct) VALUES (@QId, 'Yes', 1)", new { QId = questionId });

        Test? test = await _repository.GetTestByIdAsync(testId);

        Assert.That(test, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.Name, Is.EqualTo("My Test"));
            Assert.That(test.Questions, Has.Count.EqualTo(1));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.Questions[0].Text, Is.EqualTo("Question 1"));
            Assert.That(test.Questions[0].Options, Has.Count.EqualTo(1));
        }

        Assert.That(test.Questions[0].Options[0].OptionText, Is.EqualTo("Yes"));
    }

    [Test]
    public async Task GetTestByIdAsync_TestDoesNotExist_ReturnsNull()
    {
        Test? test = await _repository.GetTestByIdAsync(999);
        Assert.That(test, Is.Null);
    }

    [Test]
    public async Task DeleteTestAsync_TestExists_DeletesTest()
    {
        int subjectId = await CreateSubject("Maths");
        await Connection.ExecuteAsync("INSERT INTO Test (subject_id, name) VALUES (@SubId, 'To Delete')", new { SubId = subjectId });
        int testId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        await _repository.DeleteTestAsync(testId);

        int count = await Connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Test WHERE test_id = @TestId", new { TestId = testId });
        Assert.That(count, Is.Zero);
    }

    [Test]
    public async Task HasAttemptsAsync_NoAttempts_ReturnsFalse()
    {
        int subjectId = await CreateSubject("Maths");
        await Connection.ExecuteAsync("INSERT INTO Test (subject_id, name) VALUES (@SubId, 'No Attempts')", new { SubId = subjectId });
        int testId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        bool result = await _repository.HasAttemptsAsync(testId);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HasAttemptsAsync_WithAttempts_ReturnsTrue()
    {
        int subjectId = await CreateSubject("Maths");
        await Connection.ExecuteAsync("INSERT INTO Test (subject_id, name) VALUES (@SubId, 'Has Attempts')", new { SubId = subjectId });
        int testId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        // Create user and student first due to foreign key constraints
        await Connection.ExecuteAsync("INSERT INTO User (email, name, password_hash) VALUES ('student@test.com', 'Student', 'hash')");
        int userId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");
        await Connection.ExecuteAsync("INSERT INTO Student (user_id) VALUES (@UserId)", new { UserId = userId });
        int studentId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        await Connection.ExecuteAsync("INSERT INTO TestResult (test_id, student_id, questions_correct) VALUES (@TestId, @StudentId, 0)", new { TestId = testId, StudentId = studentId });

        bool result = await _repository.HasAttemptsAsync(testId);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task UpdateTestAsync_UpdatesDetailsAndQuestions()
    {
        int subjectId = await CreateSubject("Maths");
        Test initialTest = new()
        {
            SubjectId = subjectId,
            Name = "Initial Name",
            Questions =
            [
                new Question
                {
                    Text = "Old Question",
                    Options = [new QuestionOption { OptionText = "A", IsCorrect = true }]
                }
            ]
        };
        await _repository.CreateTestAsync(initialTest);
        Test createdTest = (await _repository.GetAllTestsAsync(subjectId)).First();
        int testId = createdTest.TestId;

        Test updatedTest = new()
        {
            TestId = testId,
            SubjectId = subjectId,
            Name = "Updated Name",
            Questions =
            [
                new Question
                {
                    Text = "Old Question",
                    Options = [new QuestionOption { OptionText = "A", IsCorrect = true }]
                },
                new Question
                {
                    Text = "New Question",
                    Options = [new QuestionOption { OptionText = "B", IsCorrect = true }]
                }
            ]
        };

        await _repository.UpdateTestAsync(updatedTest);

        Test? result = await _repository.GetTestByIdAsync(testId);
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Name, Is.EqualTo("Updated Name"));
            Assert.That(result.Questions, Has.Count.EqualTo(2));
        }

        int oldQuestionId = createdTest.Questions[0].QuestionId;
        Assert.That(result.Questions.Any(q => q.QuestionId == oldQuestionId), Is.True);
    }

    [Test]
    public async Task UpdateTestAsync_RemovesOrphanedQuestions()
    {
        int subjectId = await CreateSubject("Orphan");
        Test test = new()
        {
            SubjectId = subjectId,
            Name = "Orphan Test",
            Questions = [new Question { Text = "Q1", Options = [new QuestionOption { OptionText = "O", IsCorrect = true }] }]
        };
        await _repository.CreateTestAsync(test);
        Test createdTest = (await _repository.GetAllTestsAsync(subjectId)).First();
        int questionId = createdTest.Questions[0].QuestionId;

        Test updateModel = new()
        {
            TestId = createdTest.TestId,
            SubjectId = subjectId,
            Name = "Orphan Test",
            Questions = [new Question { Text = "Q2", Options = [new QuestionOption { OptionText = "O", IsCorrect = true }] }]
        };

        await _repository.UpdateTestAsync(updateModel);

        int q1Count = await Connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Question WHERE question_id = @Id", new { Id = questionId });
        Assert.That(q1Count, Is.Zero);
    }

    [Test]
    public async Task UpdateTestAsync_DoesNotRemoveSharedQuestions()
    {
        int subjectId = await CreateSubject("Shared");

        Test test1 = new()
        {
            SubjectId = subjectId,
            Name = "Test 1",
            Questions = [new Question { Text = "Shared Q", Options = [new QuestionOption { OptionText = "O", IsCorrect = true }] }]
        };
        await _repository.CreateTestAsync(test1);
        Test createdTest1 = (await _repository.GetAllTestsAsync(subjectId)).First(t => t.Name == "Test 1");
        int sharedQuestionId = createdTest1.Questions[0].QuestionId;

        // Create test 2 that uses the same question
        await Connection.ExecuteAsync("INSERT INTO Test (subject_id, name) VALUES (@SubId, 'Test 2')", new { SubId = subjectId });
        int test2Id = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");
        await Connection.ExecuteAsync("INSERT INTO TestQuestion (test_id, question_id) VALUES (@TestId, @QId)", new { TestId = test2Id, QId = sharedQuestionId });

        // Remove the question from test 1
        Test updateModel = new()
        {
            TestId = createdTest1.TestId,
            SubjectId = subjectId,
            Name = "Test 1 Updated",
            Questions = [new Question { Text = "Q2", Options = [new QuestionOption { OptionText = "O", IsCorrect = true }] }]
        };

        await _repository.UpdateTestAsync(updateModel);

        // The question should still exist as it is used by the other test
        int q1Count = await Connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Question WHERE question_id = @Id", new { Id = sharedQuestionId });
        Assert.That(q1Count, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllTestsAsync_ClosedConnection_OpensConnection()
    {
        int subjectId = await CreateSubject("GetAllOpen");
        await Connection.ExecuteAsync("INSERT INTO Test (subject_id, name) VALUES (@SubId, 'Test A')", new { SubId = subjectId });

        await Connection.CloseAsync();

        Test[] tests = (await _repository.GetAllTestsAsync()).ToArray();

        Assert.That(tests, Is.Not.Empty);
        Assert.That(tests.Any(t => t.Name == "Test A"), Is.True);
    }

    [Test]
    public async Task UpdateTestAsync_ClosedConnection_OpensConnection()
    {
        int subjectId = await CreateSubject("UpdateOpen");
        await Connection.ExecuteAsync("INSERT INTO Test (subject_id, name) VALUES (@SubId, 'Original Name')", new { SubId = subjectId });
        int testId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        await Connection.CloseAsync();

        Test updatedTest = new()
        {
            TestId = testId,
            SubjectId = subjectId,
            Name = "Updated Name",
            Questions = []
        };

        await _repository.UpdateTestAsync(updatedTest);

        Test? dbTest = await Connection.QuerySingleOrDefaultAsync<Test>("SELECT * FROM Test WHERE test_id = @TestId", new { TestId = testId });
        Assert.That(dbTest, Is.Not.Null);
        Assert.That(dbTest.Name, Is.EqualTo("Updated Name"));
    }

    [Test]
    public async Task UpdateTestAsync_DbError_RollsBackTransaction()
    {
        int subjectId = await CreateSubject("UpdateRollback");
        Test initialTest = new()
        {
            SubjectId = subjectId,
            Name = "Initial Name",
            Questions =
            [
                new Question { Text = "Valid Question", Options = [new QuestionOption { OptionText = "Valid Option", IsCorrect = true }] }
            ]
        };
        await _repository.CreateTestAsync(initialTest);

        Test createdTest = (await _repository.GetAllTestsAsync(subjectId)).First(t => t.Name == "Initial Name");

        // Attempt to update with erroneous data
        Test updatedTestWithFailure = new()
        {
            TestId = createdTest.TestId,
            SubjectId = subjectId,
            Name = "Name After Failed Update",
            Questions =
            [
                new Question { Text = "Valid Question", Options = [new QuestionOption { OptionText = "Valid Option", IsCorrect = true }] },
                new Question { Text = "New Invalid Question", Options = [new QuestionOption { OptionText = null!, IsCorrect = false }] }
            ]
        };

        Assert.ThrowsAsync<MySqlException>(async () => await _repository.UpdateTestAsync(updatedTestWithFailure));

        // Ensure that the database rolled back
        Test? dbTestAfterRollback = await _repository.GetTestByIdAsync(createdTest.TestId);
        Assert.That(dbTestAfterRollback, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dbTestAfterRollback.Name, Is.EqualTo("Initial Name"));
            Assert.That(dbTestAfterRollback.Questions, Has.Count.EqualTo(1));
        }
        Assert.That(dbTestAfterRollback.Questions[0].Text, Is.EqualTo("Valid Question"));
    }

    [Test]
    public async Task CreateTestAsync_DuplicateQuestion_ReusesExistingQuestion()
    {
        int subjectId = await CreateSubject("ReuseSubject");

        List<QuestionOption> options = [new() { OptionText = "O1", IsCorrect = true }];

        Test test1 = new()
        {
            SubjectId = subjectId,
            Name = "Test 1",
            Questions = [new Question { Text = "Shared Q", Options = options.AsReadOnly() }]
        };

        await _repository.CreateTestAsync(test1);

        Test createdTest1 = (await _repository.GetAllTestsAsync(subjectId)).First(t => t.Name == "Test 1");
        int questionId1 = createdTest1.Questions[0].QuestionId;

        // Create duplicate question in Test 2
        Test test2 = new()
        {
            SubjectId = subjectId,
            Name = "Test 2",
            Questions = [new Question { Text = "Shared Q", Options = options.AsReadOnly() }]
        };

        await _repository.CreateTestAsync(test2);

        Test createdTest2 = (await _repository.GetAllTestsAsync(subjectId)).First(t => t.Name == "Test 2");
        int questionId2 = createdTest2.Questions[0].QuestionId;

        Assert.That(questionId2, Is.EqualTo(questionId1));

        int questionCount = await Connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Question WHERE text = 'Shared Q'");
        Assert.That(questionCount, Is.EqualTo(1));
    }

    [Test]
    public async Task CreateTestAsync_DifferentOptions_CreatesNewQuestion()
    {
        int subjectId = await CreateSubject("DiffOptionsSubject");

        Test test1 = new()
        {
            SubjectId = subjectId,
            Name = "Test 1",
            Questions = [new Question { Text = "Similar Q", Options = [new QuestionOption { OptionText = "A", IsCorrect = true }] }]
        };

        await _repository.CreateTestAsync(test1);

        // Same text, different options
        Test test2 = new()
        {
            SubjectId = subjectId,
            Name = "Test 2",
            Questions = [new Question { Text = "Similar Q", Options = [new QuestionOption { OptionText = "B", IsCorrect = true }] }]
        };

        await _repository.CreateTestAsync(test2);

        int questionCount = await Connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Question WHERE text = 'Similar Q'");
        Assert.That(questionCount, Is.EqualTo(2));
    }

    [Test]
    public async Task GetUpcomingTestsForStudentAsync_ReturnsOnlyUncompletedTestsForAssignedSubjects()
    {
        // Create Subjects
        int subject1Id = await CreateSubject("Subject 1");
        int subject2Id = await CreateSubject("Subject 2");
        int subject3Id = await CreateSubject("Subject 3");

        // Create Tests
        await Connection.ExecuteAsync("INSERT INTO Test (subject_id, name, number_of_questions) VALUES (@SubId, 'Test 1', 10)", new { SubId = subject1Id });
        int test1Id = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        await Connection.ExecuteAsync("INSERT INTO Test (subject_id, name, number_of_questions) VALUES (@SubId, 'Test 2', 5)", new { SubId = subject1Id });
        int test2Id = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        await Connection.ExecuteAsync("INSERT INTO Test (subject_id, name, number_of_questions) VALUES (@SubId, 'Test 3', 8)", new { SubId = subject2Id });
        int test3Id = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        await Connection.ExecuteAsync("INSERT INTO Test (subject_id, name, number_of_questions) VALUES (@SubId, 'Test 4', 12)", new { SubId = subject3Id });

        // Create Student and assign to Subject 1 and Subject 2
        await Connection.ExecuteAsync("INSERT INTO User (email, name, password_hash) VALUES ('upcoming@test.com', 'Student', 'hash')");
        int userId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");
        await Connection.ExecuteAsync("INSERT INTO Student (user_id) VALUES (@UserId)", new { UserId = userId });
        int studentId = await Connection.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()");

        await Connection.ExecuteAsync("INSERT INTO StudentSubject (student_id, subject_id) VALUES (@StudentId, @SubjectId)", new { StudentId = studentId, SubjectId = subject1Id });
        await Connection.ExecuteAsync("INSERT INTO StudentSubject (student_id, subject_id) VALUES (@StudentId, @SubjectId)", new { StudentId = studentId, SubjectId = subject2Id });

        // Mark Test 1 as completed
        await Connection.ExecuteAsync("INSERT INTO TestResult (test_id, student_id, questions_correct) VALUES (@TestId, @StudentId, 5)", new { TestId = test1Id, StudentId = studentId });

        List<UpcomingTestResponse> result = (await _repository.GetUpcomingTestsForStudentAsync(studentId)).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Any(t => t.TestId == test2Id && t is { Name: "Test 2", Subject: "Subject 1", NumberOfQuestions: 5 }), Is.True);
            Assert.That(result.Any(t => t.TestId == test3Id && t is { Name: "Test 3", Subject: "Subject 2", NumberOfQuestions: 8 }), Is.True);
        }
    }
}
