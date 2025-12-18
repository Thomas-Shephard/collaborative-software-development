using Jahoot.WebApi.Settings;

namespace Jahoot.WebApi.Tests.Settings;

public class SecurityLockoutSettingsTests
{
    private const int MaxFailedAttempts = 5;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);
    private readonly TimeSpan _failedAttemptResetInterval = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _incrementalLockoutDuration = TimeSpan.FromMinutes(2);
    private readonly TimeSpan _initialLockoutDuration = TimeSpan.FromMinutes(1);

    [Test]
    public void SecurityLockoutSettings_CanBeInstantiated_WithValidProperties()
    {
        SecurityLockoutSettings settings = new()
        {
            MaxFailedAttempts = MaxFailedAttempts,
            InitialLockoutDuration = _initialLockoutDuration,
            IncrementalLockoutDuration = _incrementalLockoutDuration,
            FailedAttemptResetInterval = _failedAttemptResetInterval,
            CleanupInterval = _cleanupInterval
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(settings.MaxFailedAttempts, Is.EqualTo(MaxFailedAttempts));
            Assert.That(settings.InitialLockoutDuration, Is.EqualTo(_initialLockoutDuration));
            Assert.That(settings.IncrementalLockoutDuration, Is.EqualTo(_incrementalLockoutDuration));
            Assert.That(settings.FailedAttemptResetInterval, Is.EqualTo(_failedAttemptResetInterval));
            Assert.That(settings.CleanupInterval, Is.EqualTo(_cleanupInterval));
        }
    }

    [Test]
    public void SecurityLockoutSettings_MaxFailedAttemptsIsZero_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = new SecurityLockoutSettings
            {
                MaxFailedAttempts = 0,
                InitialLockoutDuration = _initialLockoutDuration,
                IncrementalLockoutDuration = _incrementalLockoutDuration,
                FailedAttemptResetInterval = _failedAttemptResetInterval,
                CleanupInterval = _cleanupInterval
            };
        });
    }

    [Test]
    public void SecurityLockoutSettings_InitialLockoutDurationIsZero_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = new SecurityLockoutSettings
            {
                MaxFailedAttempts = MaxFailedAttempts,
                InitialLockoutDuration = TimeSpan.Zero,
                IncrementalLockoutDuration = _incrementalLockoutDuration,
                FailedAttemptResetInterval = _failedAttemptResetInterval,
                CleanupInterval = _cleanupInterval
            };
        });
    }

    [Test]
    public void SecurityLockoutSettings_IncrementalLockoutDurationIsNegative_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = new SecurityLockoutSettings
            {
                MaxFailedAttempts = MaxFailedAttempts,
                InitialLockoutDuration = _initialLockoutDuration,
                IncrementalLockoutDuration = TimeSpan.FromMinutes(-2),
                FailedAttemptResetInterval = _failedAttemptResetInterval,
                CleanupInterval = _cleanupInterval
            };
        });
    }

    [Test]
    public void SecurityLockoutSettings_FailedAttemptResetIntervalIsNegative_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = new SecurityLockoutSettings
            {
                MaxFailedAttempts = MaxFailedAttempts,
                InitialLockoutDuration = _initialLockoutDuration,
                IncrementalLockoutDuration = _incrementalLockoutDuration,
                FailedAttemptResetInterval = TimeSpan.FromMinutes(-5),
                CleanupInterval = _cleanupInterval
            };
        });
    }

    [Test]
    public void SecurityLockoutSettings_CleanupIntervalIsZero_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = new SecurityLockoutSettings
            {
                MaxFailedAttempts = MaxFailedAttempts,
                InitialLockoutDuration = _initialLockoutDuration,
                IncrementalLockoutDuration = _incrementalLockoutDuration,
                FailedAttemptResetInterval = _failedAttemptResetInterval,
                CleanupInterval = TimeSpan.Zero
            };
        });
    }
}
