using System.Collections.Concurrent;

namespace Jahoot.WebApi.Services;

public class LoginAttemptService : TimedBackgroundService, ILoginAttemptService
{
    private const int MaxFailedAttempts = 3;
    private static readonly TimeSpan InitialLockoutDuration = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan LockoutDurationIncrease = TimeSpan.FromSeconds(5);

    private readonly ConcurrentDictionary<string, int> _failedAttempts = new();
    private readonly ConcurrentDictionary<string, int> _lockoutCount = new();
    private readonly ConcurrentDictionary<string, DateTime> _lockouts = new();

    public LoginAttemptService(TimeSpan? cleanupInterval = null)
    {
        cleanupInterval ??= TimeSpan.FromMinutes(1);
        InitializeTimer(CleanupExpiredLockouts, cleanupInterval.Value);
    }

    public Task<bool> IsLockedOut(string username, string ipAddress)
    {
        string userKey = GetUserKey(username);
        string ipKey = GetIpKey(ipAddress);

        return Task.FromResult((_lockouts.TryGetValue(userKey, out DateTime userExpiry) && userExpiry > DateTime.UtcNow) ||
                               (_lockouts.TryGetValue(ipKey, out DateTime ipExpiry) && ipExpiry > DateTime.UtcNow));
    }

    public Task RecordFailedLoginAttempt(string username, string ipAddress)
    {
        string userKey = GetUserKey(username);
        string ipKey = GetIpKey(ipAddress);

        HandleFailedAttempt(userKey);
        HandleFailedAttempt(ipKey);

        return Task.CompletedTask;
    }

    public void ResetFailedLoginAttempts(string username, string ipAddress)
    {
        string userKey = GetUserKey(username);
        string ipKey = GetIpKey(ipAddress);

        _failedAttempts.TryRemove(userKey, out _);
        _failedAttempts.TryRemove(ipKey, out _);
        _lockoutCount.TryRemove(userKey, out _);
        _lockoutCount.TryRemove(ipKey, out _);
    }

    private void HandleFailedAttempt(string key)
    {
        int attemptCount = _failedAttempts.AddOrUpdate(key, 1, (_, count) => count + 1);

        if (attemptCount < MaxFailedAttempts)
        {
            return;
        }

        int lockoutNumber = _lockoutCount.AddOrUpdate(key, 1, (_, count) => count + 1);
        TimeSpan lockoutDuration = InitialLockoutDuration + (lockoutNumber - 1) * LockoutDurationIncrease;
        DateTime lockoutExpiry = DateTime.UtcNow + lockoutDuration;
        _lockouts.AddOrUpdate(key, lockoutExpiry, (_, _) => lockoutExpiry);
    }

    private void CleanupExpiredLockouts(object? state)
    {
        DateTime now = DateTime.UtcNow;

        foreach ((string key, _) in _lockouts.Where(pair => pair.Value < now).ToList())
        {
            _lockouts.TryRemove(key, out _);
            _failedAttempts.TryRemove(key, out _);
            _lockoutCount.TryRemove(key, out _);
        }
    }

    private static string GetUserKey(string username)
    {
        return $"user:{username}";
    }

    private static string GetIpKey(string ipAddress)
    {
        return $"ip:{ipAddress}";
    }
}
