using System.Data;
using Dapper;

namespace Jahoot.WebApi.Repositories;

public class StudentRepository(IDbConnection connection) : IStudentRepository
{
    private const string CreateStudentQuery = "INSERT INTO Student (user_id) VALUES (@UserId);";

    public async Task CreateStudentAsync(string name, string email, string hashedPassword)
    {
        using IDbTransaction transaction = connection.BeginTransaction();

        const string createUserQuery = "INSERT INTO User (name, email, password_hash) VALUES (@Name, @Email, @HashedPassword); SELECT LAST_INSERT_ID();";
        int userId = await connection.ExecuteScalarAsync<int>( createUserQuery, new { Name = name, Email = email, HashedPassword = hashedPassword }, transaction);

        await connection.ExecuteAsync(CreateStudentQuery, new { UserId = userId }, transaction);

        transaction.Commit();
    }

    public async Task CreateStudentAsync(int userId)
    {
        await connection.ExecuteAsync(CreateStudentQuery, new { UserId = userId });
    }
}
