using System.Collections.Concurrent;
using Jahoot.WebApi.Settings;

namespace Jahoot.WebApi.Services;

public class TokenDenyService : TimedBackgroundService, ITokenDenyService
{
    private readonly ConcurrentDictionary<string, DateTime> _denylist = new();

    public TokenDenyService(TokenDenySettings settings, TimeProvider timeProvider) : base(timeProvider)
    {
        InitializeTimer(CleanupExpiredTokens, settings.CleanupInterval);
    }

    public Task DenyAsync(string jti, DateTime expiry)
    {
        _denylist.TryAdd(jti, expiry);
        return Task.CompletedTask;
    }

    /// <summary>
    /// This checks if a token is denied.
    /// </summary>
    /// <param name="jti">The token's unique identifier.</param>
    /// <returns>True if the token is denied.</returns>
    public Task<bool> IsDeniedAsync(string jti)
    {
        return Task.FromResult(_denylist.ContainsKey(jti));
    }

    private void CleanupExpiredTokens(object? state)
    {
        DateTime now = TimeProvider.GetUtcNow().UtcDateTime;

        foreach ((string expiredToken, _) in _denylist.Where(pair => pair.Value < now).ToList())
        {
            _denylist.TryRemove(expiredToken, out _);
        }
    }
}
