using System.Data;
using Dapper;
using Jahoot.Core.Models;

namespace Jahoot.WebApi.Repositories;

public class StudentRepository(IDbConnection connection) : IStudentRepository
{
    private const string CreateStudentQuery = "INSERT INTO Student (user_id) VALUES (@UserId);";

    private sealed class StudentData
    {
        public int UserId { get; init; }
        public required string Email { get; init; }
        public required string Name { get; init; }
        public required string PasswordHash { get; init; }
        public DateTime? LastLogin { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public int StudentId { get; init; }
        public required string AccountStatus { get; init; }
    }

    public async Task CreateStudentAsync(string name, string email, string hashedPassword)
    {
        using IDbTransaction transaction = connection.BeginTransaction();

        const string createUserQuery = "INSERT INTO User (name, email, password_hash) VALUES (@Name, @Email, @HashedPassword); SELECT LAST_INSERT_ID();";
        int userId = await connection.ExecuteScalarAsync<int>(createUserQuery, new { Name = name, Email = email, HashedPassword = hashedPassword }, transaction);

        await connection.ExecuteAsync(CreateStudentQuery, new { UserId = userId }, transaction);

        transaction.Commit();
    }

    public async Task CreateStudentAsync(int userId)
    {
        await connection.ExecuteAsync(CreateStudentQuery, new { UserId = userId });
    }

    public async Task<Student?> GetStudentByUserIdAsync(int userId)
    {
        const string query = """
                             SELECT user.*, student.*, lecturer.is_admin
                             FROM Student student
                                      JOIN User user ON student.user_id = user.user_id
                                      LEFT JOIN Lecturer lecturer ON user.user_id = lecturer.user_id
                             WHERE student.user_id = @UserId
                             """;

        IEnumerable<Student> students = await connection.QueryAsync<StudentData, bool?, Student>(
            query,
            (studentData, isAdmin) =>
            {
                List<Role> roles = [Role.Student];

                if (isAdmin.HasValue)
                {
                    roles.Add(Role.Lecturer);
                    if (isAdmin.Value)
                    {
                        roles.Add(Role.Admin);
                    }
                }

                // Map database enum string to C# enum
                // The database enum values are 'pending_approval', 'active', 'disabled'
                // The C# enum values are PendingApproval, Active, Disabled
                StudentAccountStatus status = studentData.AccountStatus switch
                {
                    "pending_approval" => StudentAccountStatus.PendingApproval,
                    "active" => StudentAccountStatus.Active,
                    "disabled" => StudentAccountStatus.Disabled,
                    _ => throw new InvalidOperationException($"Unknown account status: {studentData.AccountStatus}")
                };

                return new Student
                {
                    UserId = studentData.UserId,
                    Email = studentData.Email,
                    Name = studentData.Name,
                    PasswordHash = studentData.PasswordHash,
                    Roles = roles,
                    LastLogin = studentData.LastLogin,
                    CreatedAt = studentData.CreatedAt,
                    UpdatedAt = studentData.UpdatedAt,
                    StudentId = studentData.StudentId,
                    AccountStatus = status
                };
            },
            new { UserId = userId },
            splitOn: "is_admin");

        return students.SingleOrDefault();
    }
}
