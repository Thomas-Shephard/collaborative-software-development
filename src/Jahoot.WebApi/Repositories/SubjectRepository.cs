using System.Data;
using Dapper;
using Jahoot.Core.Models;
using Jahoot.WebApi.Models.Responses;

namespace Jahoot.WebApi.Repositories;

public class SubjectRepository(IDbConnection connection) : ISubjectRepository
{
    public async Task CreateSubjectAsync(string name)
    {
        const string query = "INSERT INTO Subject (name) VALUES (@Name);";
        await connection.ExecuteAsync(query, new { Name = name });
    }

    public async Task<IEnumerable<Subject>> GetAllSubjectsAsync(bool? isActive = null)
    {
        const string baseQuery = "SELECT * FROM Subject";
        const string filteredQuery = "SELECT * FROM Subject WHERE is_active = @IsActive";
        string query = isActive.HasValue ? filteredQuery : baseQuery;
        return await connection.QueryAsync<Subject>(query, new { IsActive = isActive });
    }

    public async Task<Subject?> GetSubjectByIdAsync(int id)
    {
        const string query = "SELECT * FROM Subject WHERE subject_id = @Id";
        return await connection.QuerySingleOrDefaultAsync<Subject>(query, new { Id = id });
    }

    public async Task<IEnumerable<Subject>> GetSubjectsByIdsAsync(IEnumerable<int> ids)
    {
        const string query = "SELECT * FROM Subject WHERE subject_id IN @Ids";
        return await connection.QueryAsync<Subject>(query, new { Ids = ids });
    }

    public async Task<Subject?> GetSubjectByNameAsync(string name)
    {
        const string query = "SELECT * FROM Subject WHERE name = @Name";
        return await connection.QuerySingleOrDefaultAsync<Subject>(query, new { Name = name });
    }

    public async Task<IEnumerable<LeaderboardEntry>> GetLeaderboardForSubjectAsync(int subjectId)
    {
        const string query = """
                             SELECT
                                 user.name AS StudentName,
                                 SUM(test_result.score) AS TotalScore
                             FROM TestResult test_result
                                 JOIN Test test ON test_result.test_id = test.test_id
                                 JOIN Student student ON test_result.student_id = student.student_id
                                 JOIN User user ON student.user_id = user.user_id
                             WHERE test.subject_id = @SubjectId AND user.is_disabled = 0
                             GROUP BY student.student_id, user.name
                             ORDER BY TotalScore DESC
                             LIMIT 5
                             """;

        IEnumerable<LeaderboardData> results = await connection.QueryAsync<LeaderboardData>(query, new { SubjectId = subjectId });

        return results.Select((row, index) => new LeaderboardEntry
        {
            Rank = index + 1,
            StudentName = row.StudentName,
            TotalScore = row.TotalScore
        });
    }

    public async Task UpdateSubjectAsync(Subject subject)
    {
        const string query = "UPDATE Subject SET name = @Name, is_active = @IsActive WHERE subject_id = @SubjectId";
        await connection.ExecuteAsync(query, subject);
    }

    private sealed class LeaderboardData
    {
        public required string StudentName { get; init; }
        public long TotalScore { get; init; }
    }
}
