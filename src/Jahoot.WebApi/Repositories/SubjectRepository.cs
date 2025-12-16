using System.Data;
using Dapper;
using Jahoot.Core.Models;

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

    public async Task<Subject?> GetSubjectByNameAsync(string name)
    {
        const string query = "SELECT * FROM Subject WHERE name = @Name";
        return await connection.QuerySingleOrDefaultAsync<Subject>(query, new { Name = name });
    }

    public async Task UpdateSubjectAsync(Subject subject)
    {
        const string query = "UPDATE Subject SET name = @Name, is_active = @IsActive WHERE subject_id = @SubjectId";
        await connection.ExecuteAsync(query, subject);
    }
}
