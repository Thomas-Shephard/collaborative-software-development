using System.Collections.Concurrent;
using Jahoot.WebApi.Settings;

namespace Jahoot.WebApi.Services;

public class LoginAttemptService : TimedBackgroundService, ILoginAttemptService
{
    private readonly ConcurrentDictionary<string, DateTime> _emailLockouts = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, (int Count, DateTime LastAttempt)> _failedEmailAttempts = new(StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentDictionary<string, (int Count, DateTime LastAttempt)> _failedIpAttempts = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, DateTime> _ipLockouts = new(StringComparer.Ordinal);

    private readonly LoginAttemptSettings _settings;

    public LoginAttemptService(LoginAttemptSettings settings, TimeProvider timeProvider) : base(timeProvider)
    {
        _settings = settings;
        InitializeTimer(CleanupExpiredLockoutsAndStaleAttempts, _settings.CleanupInterval);
    }

    public Task<bool> IsLockedOut(string email, string ipAddress)
    {
        return Task.FromResult((_emailLockouts.TryGetValue(email, out DateTime emailExpiry) && emailExpiry > TimeProvider.GetUtcNow().UtcDateTime) ||
                               (_ipLockouts.TryGetValue(ipAddress, out DateTime ipExpiry) && ipExpiry > TimeProvider.GetUtcNow().UtcDateTime));
    }

    public Task RecordFailedLoginAttempt(string email, string ipAddress)
    {
        HandleFailedAttempt(email, _failedEmailAttempts, _emailLockouts);
        HandleFailedAttempt(ipAddress, _failedIpAttempts, _ipLockouts);

        return Task.CompletedTask;
    }

    public Task ResetFailedLoginAttempts(string email, string ipAddress)
    {
        _failedEmailAttempts.TryRemove(email, out _);
        _failedIpAttempts.TryRemove(ipAddress, out _);

        _emailLockouts.TryRemove(email, out _);
        _ipLockouts.TryRemove(ipAddress, out _);

        return Task.CompletedTask;
    }

    private void HandleFailedAttempt(string key, ConcurrentDictionary<string, (int Count, DateTime LastAttempt)> failedAttemptsDict, ConcurrentDictionary<string, DateTime> lockoutsDict)
    {
        failedAttemptsDict.AddOrUpdate(key, (1, TimeProvider.GetUtcNow().UtcDateTime), (_, existing) =>
        {
            int newAttemptCount = existing.Count + 1;

            if (newAttemptCount < _settings.MaxFailedAttempts)
            {
                return (newAttemptCount, TimeProvider.GetUtcNow().UtcDateTime);
            }

            TimeSpan lockoutDuration = _settings.InitialLockoutDuration + (newAttemptCount - _settings.MaxFailedAttempts) * _settings.IncrementalLockoutDuration;
            DateTime lockoutExpiry = TimeProvider.GetUtcNow().UtcDateTime + lockoutDuration;
            lockoutsDict.AddOrUpdate(key, lockoutExpiry, (_, _) => lockoutExpiry);

            return (newAttemptCount, TimeProvider.GetUtcNow().UtcDateTime);
        });
    }

    private void CleanupExpiredLockoutsAndStaleAttempts(object? state)
    {
        DateTime now = TimeProvider.GetUtcNow().UtcDateTime;

        CleanupDictionaries(_emailLockouts, _failedEmailAttempts, now);
        CleanupDictionaries(_ipLockouts, _failedIpAttempts, now);
    }

    private void CleanupDictionaries(ConcurrentDictionary<string, DateTime> lockouts,
                                     ConcurrentDictionary<string, (int Count, DateTime LastAttempt)> failedAttempts,
                                     DateTime now)
    {
        foreach ((string key, _) in lockouts.Where(lockout => lockout.Value < now).ToList())
        {
            lockouts.TryRemove(key, out _);
            failedAttempts.TryRemove(key, out _);
        }

        foreach ((string key, (int Count, DateTime LastAttempt) value) in failedAttempts.ToList())
        {
            if (!lockouts.ContainsKey(key) && now - value.LastAttempt > _settings.FailedAttemptResetInterval)
            {
                failedAttempts.TryRemove(key, out _);
            }
        }
    }
}
