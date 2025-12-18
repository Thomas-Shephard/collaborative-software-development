using System.Data;
using Dapper;
using Jahoot.Core.Models;
using Jahoot.WebApi.Models.Responses;

namespace Jahoot.WebApi.Repositories;

public class TestRepository(IDbConnection connection) : ITestRepository
{
    public async Task CreateTestAsync(Test test)
    {
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        using IDbTransaction transaction = connection.BeginTransaction();

        try
        {
            const string createTestQuery = "INSERT INTO Test (subject_id, name, number_of_questions) VALUES (@SubjectId, @Name, @NumberOfQuestions); SELECT LAST_INSERT_ID();";
            int testId = await connection.ExecuteScalarAsync<int>(createTestQuery, new { test.SubjectId, test.Name, test.NumberOfQuestions }, transaction);

            HashSet<int> usedQuestionIds = [];

            foreach (Question question in test.Questions)
            {
                int questionId = await GetOrCreateQuestionAsync(question, transaction, usedQuestionIds);
                usedQuestionIds.Add(questionId);

                const string linkQuery = "INSERT INTO TestQuestion (test_id, question_id) VALUES (@TestId, @QuestionId);";
                await connection.ExecuteAsync(linkQuery, new { TestId = testId, QuestionId = questionId }, transaction);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<Test>> GetAllTestsAsync(int? subjectId = null)
    {
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        const string baseQuery = "SELECT test_id FROM Test";
        const string filteredQuery = $"{baseQuery} WHERE subject_id = @SubjectId";

        string query = subjectId.HasValue ? filteredQuery : baseQuery;

        IEnumerable<int> testIds = await connection.QueryAsync<int>(query, new { SubjectId = subjectId });

        List<Test> tests = [];
        foreach (int testId in testIds)
        {
            Test? test = await GetTestInternalAsync(testId);

            if (test is not null)
            {
                tests.Add(test);
            }
        }

        return tests;
    }

    public async Task<Test?> GetTestByIdAsync(int testId)
    {
        return await GetTestInternalAsync(testId);
    }

    public async Task UpdateTestAsync(Test test)
    {
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        using IDbTransaction transaction = connection.BeginTransaction();

        try
        {
            const string updateTestQuery = "UPDATE Test SET name = @Name, subject_id = @SubjectId, number_of_questions = @NumberOfQuestions WHERE test_id = @TestId";
            await connection.ExecuteAsync(updateTestQuery, new { test.Name, test.SubjectId, test.NumberOfQuestions, test.TestId }, transaction);

            Question[] currentQuestions = (await GetQuestionsInternalAsync(test.TestId, transaction)).ToArray();
            int[] currentQuestionIds = currentQuestions.Select(question => question.QuestionId).ToArray();

            HashSet<int> newQuestionIds = [];

            foreach (Question question in test.Questions)
            {
                int questionId = await GetOrCreateQuestionAsync(question, transaction, newQuestionIds);
                newQuestionIds.Add(questionId);
            }

            // Update links to questions in this test
            const string deleteLinksQuery = "DELETE FROM TestQuestion WHERE test_id = @TestId";
            await connection.ExecuteAsync(deleteLinksQuery, new { test.TestId }, transaction);

            if (newQuestionIds.Count != 0)
            {
                const string linkQuery = "INSERT INTO TestQuestion (test_id, question_id) VALUES (@TestId, @QuestionId);";
                await connection.ExecuteAsync(linkQuery, newQuestionIds.Select(questionId => new { test.TestId, QuestionId = questionId }), transaction);
            }

            // Remove questions which are not used by any tests
            List<int> removedQuestionIds = currentQuestionIds.Except(newQuestionIds).ToList();
            if (removedQuestionIds.Count > 0)
            {
                const string deleteOrphansQuery = "DELETE FROM Question WHERE question_id IN @Ids AND question_id NOT IN (SELECT question_id FROM TestQuestion)";
                await connection.ExecuteAsync(deleteOrphansQuery, new { Ids = removedQuestionIds }, transaction);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task DeleteTestAsync(int testId)
    {
        const string query = "DELETE FROM Test WHERE test_id = @TestId";
        await connection.ExecuteAsync(query, new { TestId = testId });
    }

    public async Task<bool> HasAttemptsAsync(int testId)
    {
        const string query = "SELECT COUNT(1) FROM TestResult WHERE test_id = @TestId";
        int count = await connection.ExecuteScalarAsync<int>(query, new { TestId = testId });
        return count > 0;
    }

    public async Task<IEnumerable<UpcomingTestResponse>> GetUpcomingTestsForStudentAsync(int studentId)
    {
        const string query = """
                             SELECT test.test_id, test.name, subject.name AS Subject, test.number_of_questions
                             FROM Test test
                                 JOIN Subject subject ON test.subject_id = subject.subject_id
                                 JOIN StudentSubject student_subject ON subject.subject_id = student_subject.subject_id
                                 LEFT JOIN TestResult test_result ON test.test_id = test_result.test_id AND student_subject.student_id = test_result.student_id
                             WHERE student_subject.student_id = @StudentId AND test_result.test_result_id IS NULL
                             """;

        return await connection.QueryAsync<UpcomingTestResponse>(query, new { StudentId = studentId });
    }

    public async Task<IEnumerable<CompletedTestResponse>> GetCompletedTestsForStudentAsync(int studentId)
    {
        const string query = """
                             SELECT
                                 test.test_id AS TestId,
                                 test.name AS TestName,
                                 subject.name AS SubjectName,
                                 test_result.completion_date AS CompletedDate,
                                 (test_result.questions_correct * 100.0 / test.number_of_questions) AS ScorePercentage,
                                 test_result.score AS TotalPoints
                             FROM TestResult test_result
                                 JOIN Test test ON test_result.test_id = test.test_id
                                 JOIN Subject subject ON test.subject_id = subject.subject_id
                             WHERE test_result.student_id = @StudentId AND subject.is_active = TRUE
                             ORDER BY test_result.completion_date DESC
                             """;

        return await connection.QueryAsync<CompletedTestResponse>(query, new { StudentId = studentId });
    }

    public async Task<StudentStatisticsResponse> GetStudentStatisticsAsync(int studentId)
    {
        const string query = """
                             SELECT
                                 test_result.completion_date AS Date,
                                 (test_result.questions_correct * 100.0 / test.number_of_questions) AS ScorePercentage,
                                 test_result.score AS TotalPoints
                             FROM TestResult test_result
                                 JOIN Test test ON test_result.test_id = test.test_id
                                 JOIN Subject subject ON test.subject_id = subject.subject_id
                             WHERE test_result.student_id = @StudentId AND subject.is_active = TRUE
                             ORDER BY test_result.completion_date
                             """;

        List<TestResultStatsDto> results = (await connection.QueryAsync<TestResultStatsDto>(query, new { StudentId = studentId })).ToList();

        if (results.Count == 0)
        {
            return new StudentStatisticsResponse();
        }

        int totalPoints = results.Sum(r => r.TotalPoints);
        double averageScore = (double)results.Average(r => r.ScorePercentage);

        List<ScoreHistoryItem> history = results.Select(r => new ScoreHistoryItem
        {
            Date = r.Date,
            ScorePercentage = (double)r.ScorePercentage
        }).ToList();

        return new StudentStatisticsResponse
        {
            TotalPoints = totalPoints,
            AverageScorePercentage = averageScore,
            ScoreHistory = history
        };
    }

    private sealed class TestResultStatsDto
    {
        public DateTime Date { get; init; }
        public decimal ScorePercentage { get; init; }
        public int TotalPoints { get; init; }
    }

    private async Task<Test?> GetTestInternalAsync(int testId)
    {
        const string query = "SELECT * FROM Test WHERE test_id = @TestId";
        Test? test = await connection.QuerySingleOrDefaultAsync<Test>(query, new { TestId = testId });

        if (test == null)
        {
            return null;
        }

        IEnumerable<Question> questions = await GetQuestionsInternalAsync(testId, null);

        return new Test
        {
            TestId = test.TestId,
            SubjectId = test.SubjectId,
            Name = test.Name,
            NumberOfQuestions = test.NumberOfQuestions,
            CreatedAt = test.CreatedAt,
            UpdatedAt = test.UpdatedAt,
            Questions = questions.ToList().AsReadOnly()
        };
    }

    private async Task<IEnumerable<Question>> GetQuestionsInternalAsync(int testId, IDbTransaction? transaction)
    {
        const string questionsQuery = "SELECT question.* FROM Question question JOIN TestQuestion test_question ON question.question_id = test_question.question_id WHERE test_question.test_id = @TestId";
        List<Question> questions = (await connection.QueryAsync<Question>(questionsQuery, new { TestId = testId }, transaction)).ToList();

        if (questions.Count == 0)
        {
            return [];
        }

        IEnumerable<int> questionIds = questions.Select(question => question.QuestionId);
        const string questionOptionsQuery = "SELECT * FROM QuestionOption WHERE question_id IN @Ids";
        IEnumerable<QuestionOption> questionOptions = await connection.QueryAsync<QuestionOption>(questionOptionsQuery, new { Ids = questionIds }, transaction);

        ILookup<int, QuestionOption> optionsLookup = questionOptions.ToLookup(option => option.QuestionId);

        return questions.Select(question => new Question
        {
            QuestionId = question.QuestionId,
            Text = question.Text,
            CreatedAt = question.CreatedAt,
            UpdatedAt = question.UpdatedAt,
            Options = optionsLookup[question.QuestionId]
                                     .OrderBy(questionOption => questionOption.OptionText)
                                     .ToList()
                                     .AsReadOnly()
        });
    }

    private async Task<int> GetOrCreateQuestionAsync(Question question, IDbTransaction transaction, HashSet<int>? excludedQuestionIds = null)
    {
        const string findCandidatesQuery = @"
            SELECT q.question_id AS QuestionId, qo.option_text AS OptionText, qo.is_correct AS IsCorrect
            FROM Question q
            JOIN QuestionOption qo ON q.question_id = qo.question_id
            WHERE q.text = @Text";

        IEnumerable<QuestionOption> candidates = await connection.QueryAsync<QuestionOption>(findCandidatesQuery, new { question.Text }, transaction);

        int? questionId = (from @group in candidates.GroupBy(c => c.QuestionId)
                           where excludedQuestionIds is null || !excludedQuestionIds.Contains(@group.Key)
                               let dbOptions = @group.OrderBy(o => o.OptionText).ThenBy(o => o.IsCorrect).ToList()
                               let newOptions = question.Options.OrderBy(o => o.OptionText).ThenBy(o => o.IsCorrect).ToList()
                               where dbOptions.Count == newOptions.Count
                                   let match = !dbOptions.Where((t, i) => t.OptionText != newOptions[i].OptionText || t.IsCorrect != newOptions[i].IsCorrect).Any()
                                   where match
                                    select (int?)@group.Key
                          ).FirstOrDefault();

        if (questionId.HasValue)
        {
            return questionId.Value;
        }

        const string createQuestionQuery = "INSERT INTO Question (text) VALUES (@Text); SELECT LAST_INSERT_ID();";
        int newQuestionId = await connection.ExecuteScalarAsync<int>(createQuestionQuery, new { question.Text }, transaction);

        const string createOptionQuery = "INSERT INTO QuestionOption (question_id, option_text, is_correct) VALUES (@QuestionId, @OptionText, @IsCorrect);";
        await connection.ExecuteAsync(createOptionQuery, question.Options.Select(option => new { QuestionId = newQuestionId, option.OptionText, option.IsCorrect }), transaction);

        return newQuestionId;
    }
}
