namespace Jahoot.WebApi.Services;

public interface ITokenDenyService
{
    Task DenyAsync(string jti, DateTime expiry);
    Task<bool> IsDeniedAsync(string jti);
}
