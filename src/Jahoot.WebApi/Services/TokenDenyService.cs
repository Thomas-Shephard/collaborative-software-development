using System.Collections.Concurrent;

namespace Jahoot.WebApi.Services;

public class TokenDenyService : ITokenDenyService, IDisposable
{
    private readonly Timer _cleanupTimer;
    private readonly ConcurrentDictionary<string, DateTime> _denylist = new();

    public TokenDenyService(TimeSpan? cleanupInterval = null)
    {
        cleanupInterval ??= TimeSpan.FromHours(1);
        _cleanupTimer = new Timer(CleanupExpiredTokens, null, cleanupInterval.Value, cleanupInterval.Value);
    }

    public void Dispose()
    {
        _cleanupTimer.Dispose();
        GC.SuppressFinalize(this);
    }

    public Task DenyAsync(string jti, DateTime expiry)
    {
        _denylist.TryAdd(jti, expiry);
        return Task.CompletedTask;
    }

    public Task<bool> IsDeniedAsync(string jti)
    {
        return Task.FromResult(_denylist.ContainsKey(jti));
    }

    private void CleanupExpiredTokens(object? state)
    {
        DateTime now = DateTime.UtcNow;
        foreach ((string expiredToken, _) in _denylist.Where(pair => pair.Value < now))
        {
            _denylist.TryRemove(expiredToken, out _);
        }
    }
}
