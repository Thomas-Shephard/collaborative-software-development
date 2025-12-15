using System.Data;
using Dapper;
using Jahoot.Core.Models;

namespace Jahoot.WebApi.Repositories;

public class LecturerRepository(IDbConnection connection) : ILecturerRepository
{
    private const string CreateLecturerSql = "INSERT INTO Lecturer (user_id, is_admin) VALUES (@UserId, @IsAdmin);";

    private sealed class LecturerData
    {
        public int UserId { get; init; }
        public required string Email { get; init; }
        public required string Name { get; init; }
        public required string PasswordHash { get; init; }
        public DateTime? LastLogin { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public int LecturerId { get; init; }
        public bool IsAdmin { get; init; }
    }

    public async Task CreateLecturerAsync(string name, string email, string hashedPassword, bool isAdmin)
    {
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        using IDbTransaction transaction = connection.BeginTransaction();

        const string createUserQuery = "INSERT INTO User (name, email, password_hash) VALUES (@Name, @Email, @HashedPassword); SELECT LAST_INSERT_ID();";
        int userId = await connection.ExecuteScalarAsync<int>(createUserQuery, new { Name = name, Email = email, HashedPassword = hashedPassword }, transaction);

        await connection.ExecuteAsync(CreateLecturerSql, new { UserId = userId, IsAdmin = isAdmin }, transaction);

        transaction.Commit();
    }

    public async Task CreateLecturerAsync(int userId, bool isAdmin)
    {
        await connection.ExecuteAsync(CreateLecturerSql, new { UserId = userId, IsAdmin = isAdmin });
    }

    public async Task<Lecturer?> GetLecturerByUserIdAsync(int userId)
    {
        const string query = """
                             SELECT user.*, lecturer.*, student.student_id
                             FROM Lecturer lecturer
                                      JOIN User user ON lecturer.user_id = user.user_id
                                      LEFT JOIN Student student ON user.user_id = student.user_id
                             WHERE lecturer.user_id = @UserId
                             """;

        IEnumerable<Lecturer> lecturers = await connection.QueryAsync<LecturerData, int?, Lecturer>(query, MapLecturer, new { UserId = userId }, splitOn: "student_id");

        return lecturers.SingleOrDefault();
    }

    public async Task<IEnumerable<Lecturer>> GetLecturersAsync()
    {
        const string query = """
                             SELECT user.*, lecturer.*, student.student_id
                             FROM Lecturer lecturer
                                      JOIN User user ON lecturer.user_id = user.user_id
                                      LEFT JOIN Student student ON user.user_id = student.user_id
                             """;

        return await connection.QueryAsync<LecturerData, int?, Lecturer>(query, MapLecturer, splitOn: "student_id");
    }

    public async Task UpdateLecturerAsync(Lecturer lecturer)
    {
        const string query = "UPDATE Lecturer SET is_admin = @IsAdmin WHERE lecturer_id = @LecturerId";

        await connection.ExecuteAsync(query, new { lecturer.IsAdmin, lecturer.LecturerId });
    }

    private static Lecturer MapLecturer(LecturerData data, int? studentId)
    {
        List<Role> roles = [Role.Lecturer];
        if (data.IsAdmin) roles.Add(Role.Admin);
        if (studentId.HasValue) roles.Add(Role.Student);

        return new Lecturer
        {
            UserId = data.UserId,
            Email = data.Email,
            Name = data.Name,
            PasswordHash = data.PasswordHash,
            Roles = roles,
            LastLogin = data.LastLogin,
            CreatedAt = data.CreatedAt,
            UpdatedAt = data.UpdatedAt,
            LecturerId = data.LecturerId,
            IsAdmin = data.IsAdmin
        };
    }
}
