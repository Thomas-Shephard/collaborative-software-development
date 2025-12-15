using System.Data;
using Dapper;

namespace Jahoot.WebApi.Repositories;

public class TokenDenyRepository(IDbConnection connection) : ITokenDenyRepository
{
    public async Task DenyTokenAsync(string jti, DateTime expiresAt)
    {
        await connection.ExecuteAsync("INSERT IGNORE INTO DeniedToken (jti, expires_at) VALUES (@Jti, @ExpiresAt)", new { Jti = jti, ExpiresAt = expiresAt });
    }

    public async Task DeleteExpiredTokensAsync(DateTime now)
    {
        await connection.ExecuteAsync("DELETE FROM DeniedToken WHERE expires_at < @Now", new { Now = now });
    }

    public async Task<IEnumerable<(string Jti, DateTime ExpiresAt)>> GetActiveDeniedTokensAsync(DateTime now)
    {
        return await connection.QueryAsync<(string, DateTime)>("SELECT jti, expires_at FROM DeniedToken WHERE expires_at > @Now", new { Now = now });
    }
}
