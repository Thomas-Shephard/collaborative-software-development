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
        public bool IsDisabled { get; init; }
        public DateTime? LastLogin { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }

    private const string BaseSelectQuery = "SELECT * FROM User user";

    public async Task<User?> GetUserByEmailAsync(string email, IDbTransaction? transaction = null)
    {
        const string query = $"{BaseSelectQuery} WHERE user.email = @Email";
        return await GetUserInternalAsync(query, new { Email = email }, transaction);
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        const string query = $"{BaseSelectQuery} WHERE user.user_id = @UserId";
        return await GetUserInternalAsync(query, new { UserId = userId });
    }

    public async Task<List<Role>> GetRolesByUserIdAsync(int userId, IDbTransaction? transaction = null)
    {
        const string query = """
                             SELECT user.is_disabled, lecturer.is_admin, student.is_approved AS is_student_approved
                             FROM User user
                                      LEFT JOIN Lecturer lecturer ON user.user_id = lecturer.user_id
                                      LEFT JOIN Student student ON user.user_id = student.user_id
                             WHERE user.user_id = @UserId
                             """;

        dynamic? result = await connection.QuerySingleOrDefaultAsync<dynamic>(query, new { UserId = userId });

        if (result is null || result.is_disabled)
        {
            return [];
        }

        List<Role> roles = [];

        bool? lecturerIsAdmin = result?.is_admin;
        bool? studentIsApproved = result?.is_student_approved;

        if (lecturerIsAdmin.HasValue)
        {
            roles.Add(Role.Lecturer);
            if (lecturerIsAdmin.Value)
            {
                roles.Add(Role.Admin);
            }
        }

        if (studentIsApproved.HasValue && studentIsApproved.Value)
        {
            roles.Add(Role.Student);
        }

        return roles;
    }

    public async Task UpdateUserAsync(User user, IDbTransaction? transaction = null)
    {
        await connection.ExecuteAsync("UPDATE User SET email = @Email, name = @Name, password_hash = @PasswordHash, last_login = @LastLogin, is_disabled = @IsDisabled WHERE user_id = @UserId", user, transaction);
    }

    public async Task DeleteUserAsync(int userId)
    {
        await connection.ExecuteAsync("DELETE FROM User WHERE user_id = @UserId", new { UserId = userId });
    }

    private async Task<User?> GetUserInternalAsync(string query, object parameters, IDbTransaction? transaction = null)
    {
        UserData? userData = await connection.QuerySingleOrDefaultAsync<UserData>(query, parameters, transaction);

        if (userData is null)
        {
            return null;
        }

        List<Role> roles = await GetRolesByUserIdAsync(userData.UserId, transaction);

        return MapUser(userData, roles);
    }

    private static User MapUser(UserData userData, List<Role> roles)
    {
        return new User
        {
            UserId = userData.UserId,
            Email = userData.Email,
            Name = userData.Name,
            PasswordHash = userData.PasswordHash,
            IsDisabled = userData.IsDisabled,
            Roles = roles,
            LastLogin = userData.LastLogin,
            CreatedAt = userData.CreatedAt,
            UpdatedAt = userData.UpdatedAt
        };
    }
}
