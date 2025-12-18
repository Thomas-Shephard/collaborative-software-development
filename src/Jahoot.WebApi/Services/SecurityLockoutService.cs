using System.Collections.Concurrent;
using Jahoot.WebApi.Settings;

namespace Jahoot.WebApi.Services;

public class SecurityLockoutService : TimedBackgroundService, ISecurityLockoutService
{
    private readonly ConcurrentDictionary<string, (int Count, DateTime LastAttempt)> _failedAttempts = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, DateTime> _lockouts = new(StringComparer.OrdinalIgnoreCase);

    private readonly SecurityLockoutSettings _settings;

    public SecurityLockoutService(SecurityLockoutSettings settings, TimeProvider timeProvider) : base(timeProvider)
    {
        _settings = settings;
        InitializeTimer(CleanupExpiredLockoutsAndStaleAttempts, _settings.CleanupInterval);
    }

    public Task<bool> IsLockedOut(params string[] keys)
    {
        DateTime now = TimeProvider.GetUtcNow().UtcDateTime;
        return Task.FromResult(keys.Any(key => _lockouts.TryGetValue(key, out DateTime expiry) && expiry > now));
    }

    public Task RecordFailure(params string[] keys)
    {
        foreach (string key in keys)
        {
            HandleFailedAttempt(key);
        }

        return Task.CompletedTask;
    }

    public Task ResetAttempts(params string[] keys)
    {
        foreach (string key in keys)
        {
            _failedAttempts.TryRemove(key, out _);
            _lockouts.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    private void HandleFailedAttempt(string key)
    {
        _failedAttempts.AddOrUpdate(key, (1, TimeProvider.GetUtcNow().UtcDateTime), (_, existing) =>
        {
            int newAttemptCount = existing.Count + 1;

            if (newAttemptCount < _settings.MaxFailedAttempts)
            {
                return (newAttemptCount, TimeProvider.GetUtcNow().UtcDateTime);
            }

            TimeSpan lockoutDuration = _settings.InitialLockoutDuration + (newAttemptCount - _settings.MaxFailedAttempts) * _settings.IncrementalLockoutDuration;
            DateTime lockoutExpiry = TimeProvider.GetUtcNow().UtcDateTime + lockoutDuration;
            _lockouts.AddOrUpdate(key, lockoutExpiry, (_, _) => lockoutExpiry);

            return (newAttemptCount, TimeProvider.GetUtcNow().UtcDateTime);
        });
    }

    private void CleanupExpiredLockoutsAndStaleAttempts(object? state)
    {
        DateTime now = TimeProvider.GetUtcNow().UtcDateTime;

        foreach ((string key, _) in _lockouts.Where(lockout => lockout.Value < now).ToList())
        {
            _lockouts.TryRemove(key, out _);
            _failedAttempts.TryRemove(key, out _);
        }

        foreach ((string key, (int Count, DateTime LastAttempt) value) in _failedAttempts.ToList())
        {
            if (!_lockouts.ContainsKey(key) && now - value.LastAttempt > _settings.FailedAttemptResetInterval)
            {
                _failedAttempts.TryRemove(key, out _);
            }
        }
    }
}
