using System.Data;
using Dapper;
using Jahoot.Core.Models;

namespace Jahoot.WebApi.Repositories;

public class UserRepository(IDbConnection connection) : IUserRepository
{
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await connection.QuerySingleOrDefaultAsync<User>("SELECT * FROM User WHERE email = @Email", new { Email = email });
    }

    public async Task UpdateUserAsync(User user)
    {
        await connection.ExecuteAsync("UPDATE User SET email = @Email, name = @Name, password_hash = @PasswordHash, last_login = @LastLogin WHERE user_id = @Id", user);
    }
}
