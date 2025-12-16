using System.Data;
using Dapper;
using Jahoot.Core.Models;

namespace Jahoot.WebApi.Repositories;

public class PasswordResetRepository(IDbConnection connection, IUserRepository userRepository) : IPasswordResetRepository
{
    public async Task CreateTokenAsync(int userId, string token)
    {
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        using IDbTransaction transaction = connection.BeginTransaction();
        try
        {
            const string invalidateQuery = "UPDATE PasswordResetToken SET is_revoked = TRUE WHERE user_id = @UserId AND is_used = FALSE AND is_revoked = FALSE";
            await connection.ExecuteAsync(invalidateQuery, new { UserId = userId }, transaction);

            const string insertQuery = "INSERT INTO PasswordResetToken (user_id, token_hash) VALUES (@UserId, @Token)";
            await connection.ExecuteAsync(insertQuery, new { UserId = userId, Token = token }, transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<PasswordResetToken?> GetPasswordResetTokenByEmail(string email, IDbTransaction? transaction = null)
    {
        User? user = await userRepository.GetUserByEmailAsync(email);
        if (user == null)
        {
            return null;
        }

        const string query = "SELECT token_id, user_id, token_hash, expiration, is_used, is_revoked, created_at FROM PasswordResetToken WHERE user_id = @UserId AND is_used = FALSE AND is_revoked = FALSE ORDER BY created_at DESC LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<PasswordResetToken>(query, new { user.UserId }, transaction);
    }

    public async Task UpdatePasswordResetTokenAsync(PasswordResetToken passwordResetToken, IDbTransaction? transaction = null)
    {
        await connection.ExecuteAsync("UPDATE PasswordResetToken SET is_used = @IsUsed WHERE token_id = @TokenId", passwordResetToken, transaction);
    }
}
