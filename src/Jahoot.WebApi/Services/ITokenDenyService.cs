namespace Jahoot.WebApi.Services;

/// <summary>
/// This is the plan for how we deny tokens.
/// </summary>
public interface ITokenDenyService
{
    /// <summary>
    /// This denies a token.
    /// </summary>
    /// <param name="jti">The token's unique identifier.</param>
    /// <param name="expiry">When the token expires.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DenyAsync(string jti, DateTime expiry);
    /// <summary>
    /// This checks if a token is denied.
    /// </summary>
    /// <param name="jti">The token's unique identifier.</param>
    /// <returns>True if the token is denied.</returns>
    Task<bool> IsDeniedAsync(string jti);
}
