using System.Data;
using Dapper;
using Jahoot.Core.Models;

namespace Jahoot.WebApi.Repositories;

/// <summary>
/// This is where we get users from the database.
/// </summary>
public class UserRepository(IDbConnection connection) : IUserRepository
{
    /// <summary>
    /// This gets a user from the database by their email address.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <returns>The user if they exist.</returns>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await connection.QuerySingleOrDefaultAsync<User>("SELECT * FROM User WHERE email = @Email", new { Email = email });
    }

    /// <summary>
    /// This updates a user in the database.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task UpdateUserAsync(User user)
    {
        await connection.ExecuteAsync("UPDATE User SET email = @Email, name = @Name, password_hash = @PasswordHash, last_login = @LastLogin WHERE user_id = @UserId", user);
    }
}
