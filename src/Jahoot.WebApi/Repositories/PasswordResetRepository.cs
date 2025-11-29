using System.Data;
using Dapper;
using Jahoot.Core.Models;

namespace Jahoot.WebApi.Repositories;

public class PasswordResetRepository(IDbConnection connection) : IPasswordResetRepository
{
    public async Task CreateTokenAsync(int userId, string token)
    {
        const string query = "INSERT INTO PasswordResetToken (user_id, token) VALUES (@UserId, @Token)";
        await connection.ExecuteAsync(query, new { UserId = userId, Token = token });
    }

    public async Task<PasswordResetToken?> GetTokenByTokenAsync(string token)
    {
        const string query = "SELECT * FROM PasswordResetToken WHERE token = @Token";
        return await connection.QuerySingleOrDefaultAsync<PasswordResetToken>(query, new { Token = token });
    }

    public async Task UpdateTokenAsync(PasswordResetToken token)
    {
        const string query = "UPDATE PasswordResetToken SET is_used = @IsUsed WHERE token_id = @TokenId";
        await connection.ExecuteAsync(query, token);
    }
}
