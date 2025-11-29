using System.Data;
using Dapper;
using Jahoot.Core.Models;

namespace Jahoot.WebApi.Repositories;

public class UserRepository(IDbConnection connection) : IUserRepository
{
    private sealed class UserData
    {
        public int UserId { get; init; }
        public required string Email { get; init; }
        public required string Name { get; init; }
        public required string PasswordHash { get; init; }
        public DateTime? LastLogin { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await GetUserByConditionAsync("user.email = @Email", new { Email = email });
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await GetUserByConditionAsync("user.user_id = @UserId", new { UserId = userId });
    }

    public async Task UpdateUserAsync(User user)
    {
        await connection.ExecuteAsync("UPDATE User SET email = @Email, name = @Name, password_hash = @PasswordHash, last_login = @LastLogin WHERE user_id = @UserId", user);
    }

    private async Task<User?> GetUserByConditionAsync(string whereClause, object parameters)
    {
        string query = $"""
                        SELECT user.*, lecturer.is_admin,student.student_id
                        FROM User user
                                 LEFT JOIN Lecturer lecturer ON user.user_id = lecturer.user_id
                                 LEFT JOIN Student student ON user.user_id = student.user_id
                        WHERE {whereClause}
                        """;

        IEnumerable<User> users = await connection.QueryAsync<UserData, bool?, int?, User>(
            query,
            MapUserFromDynamicResult,
            parameters,
            splitOn: "is_admin,student_id");

        return users.SingleOrDefault();
    }

    private static User MapUserFromDynamicResult(UserData userData, bool? isAdmin, int? studentId)
    {
        List<Role> roles = [];

        if (isAdmin.HasValue)
        {
            roles.Add(Role.Lecturer);
            if (isAdmin.Value)
            {
                roles.Add(Role.Admin);
            }
        }

        if (studentId.HasValue)
        {
            roles.Add(Role.Student);
        }

        return new User
        {
            UserId = userData.UserId,
            Email = userData.Email,
            Name = userData.Name,
            PasswordHash = userData.PasswordHash,
            Roles = roles,
            LastLogin = userData.LastLogin,
            CreatedAt = userData.CreatedAt,
            UpdatedAt = userData.UpdatedAt
        };
    }
}
