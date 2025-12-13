namespace Jahoot.WebApi.Repositories;

public interface ITokenDenyRepository
{
    Task DenyTokenAsync(string jti, DateTime expiresAt);
    Task DeleteExpiredTokensAsync(DateTime now);
    Task<IEnumerable<(string Jti, DateTime ExpiresAt)>> GetActiveDeniedTokensAsync(DateTime now);
}
