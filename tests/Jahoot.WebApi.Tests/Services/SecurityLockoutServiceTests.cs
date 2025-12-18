using Jahoot.WebApi.Services;
using Jahoot.WebApi.Settings;
using Microsoft.Extensions.Time.Testing;

namespace Jahoot.WebApi.Tests.Services;

public class SecurityLockoutServiceTests
{
    private const int MaxFailedAttempts = 3;
    private const string EmailKey = "Email:test@example.com";
    private const string IpKey = "IP:127.0.0.1";
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMilliseconds(10);
    private static readonly TimeSpan CleanupInterval = LockoutDuration;
    private static readonly TimeSpan FailedAttemptResetInterval = LockoutDuration;
    private FakeTimeProvider _fakeTimeProvider;

    private SecurityLockoutService _securityLockoutService;

    [SetUp]
    public void SetUp()
    {
        _fakeTimeProvider = new FakeTimeProvider();
        SecurityLockoutSettings securityLockoutSettings = new()
        {
            MaxFailedAttempts = MaxFailedAttempts,
            InitialLockoutDuration = LockoutDuration,
            IncrementalLockoutDuration = LockoutDuration,
            CleanupInterval = CleanupInterval,
            FailedAttemptResetInterval = FailedAttemptResetInterval
        };
        _securityLockoutService = new SecurityLockoutService(securityLockoutSettings, _fakeTimeProvider);
    }

    [TearDown]
    public void TearDown()
    {
        _securityLockoutService.Dispose();
    }

    [Test]
    public async Task IsLockedOut_ShouldReturnFalse_WhenNotLockedOut()
    {
        bool result = await _securityLockoutService.IsLockedOut(EmailKey, IpKey);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task RecordFailure_ShouldLockout_AfterThreeAttempts()
    {
        for (int i = 0; i < MaxFailedAttempts; i++)
        {
            await _securityLockoutService.RecordFailure(EmailKey, IpKey);
        }

        bool result = await _securityLockoutService.IsLockedOut(EmailKey, IpKey);
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsLockedOut_ShouldReturnFalse_AfterLockoutExpires()
    {
        for (int i = 0; i < MaxFailedAttempts; i++)
        {
            await _securityLockoutService.RecordFailure(EmailKey, IpKey);
        }

        bool isLockedOut = await _securityLockoutService.IsLockedOut(EmailKey, IpKey);
        Assert.That(isLockedOut, Is.True);

        _fakeTimeProvider.Advance(LockoutDuration);

        bool isStillLockedOut = await _securityLockoutService.IsLockedOut(EmailKey, IpKey);
        Assert.That(isStillLockedOut, Is.False);
    }

    [Test]
    public async Task ResetAttempts_ShouldResetAttempts()
    {
        for (int i = 0; i < MaxFailedAttempts; i++)
        {
            await _securityLockoutService.RecordFailure(EmailKey, IpKey);
        }

        bool isLockedOut = await _securityLockoutService.IsLockedOut(EmailKey, IpKey);
        Assert.That(isLockedOut, Is.True);

        _fakeTimeProvider.Advance(LockoutDuration);

        await _securityLockoutService.ResetAttempts(EmailKey, IpKey);
        await _securityLockoutService.RecordFailure(EmailKey, IpKey);

        bool isStillLockedOut = await _securityLockoutService.IsLockedOut(EmailKey, IpKey);
        Assert.That(isStillLockedOut, Is.False);
    }

    [Test]
    public async Task RecordFailure_ShouldIncreaseLockoutDuration_WhenFailingWhileLockedOut()
    {
        for (int i = 0; i < MaxFailedAttempts; i++)
        {
            await _securityLockoutService.RecordFailure(EmailKey, IpKey);
        }

        bool isLockedOut = await _securityLockoutService.IsLockedOut(EmailKey, IpKey);
        Assert.That(isLockedOut, Is.True);

        _fakeTimeProvider.Advance(LockoutDuration);

        isLockedOut = await _securityLockoutService.IsLockedOut(EmailKey, IpKey);
        Assert.That(isLockedOut, Is.False);

        await _securityLockoutService.RecordFailure(EmailKey, IpKey);

        isLockedOut = await _securityLockoutService.IsLockedOut(EmailKey, IpKey);
        Assert.That(isLockedOut, Is.True);

        _fakeTimeProvider.Advance(LockoutDuration);

        isLockedOut = await _securityLockoutService.IsLockedOut(EmailKey, IpKey);
        Assert.That(isLockedOut, Is.True);

        _fakeTimeProvider.Advance(LockoutDuration);

        isLockedOut = await _securityLockoutService.IsLockedOut(EmailKey, IpKey);
        Assert.That(isLockedOut, Is.False);
    }

    [Test]
    public async Task CleanupExpiredLockouts_ShouldRemoveExpiredLockouts()
    {
        for (int i = 0; i < MaxFailedAttempts; i++)
        {
            await _securityLockoutService.RecordFailure(EmailKey);
        }

        Assert.That(await _securityLockoutService.IsLockedOut(EmailKey), Is.True);

        _fakeTimeProvider.Advance(LockoutDuration);
        Assert.That(await _securityLockoutService.IsLockedOut(EmailKey), Is.False);

        _fakeTimeProvider.Advance(CleanupInterval);

        // After cleanup, the attempt count should be reset.
        // Recording a single failure shouldn't lockout if it was cleaned up.
        await _securityLockoutService.RecordFailure(EmailKey);
        Assert.That(await _securityLockoutService.IsLockedOut(EmailKey), Is.False);

        for (int i = 0; i < MaxFailedAttempts - 1; i++)
        {
            await _securityLockoutService.RecordFailure(EmailKey);
        }

        Assert.That(await _securityLockoutService.IsLockedOut(EmailKey), Is.True);
    }

    [Test]
    public async Task CleanupExpiredStaleAttempts_ShouldRemoveAttemptsWithoutLockout()
    {
        for (int i = 0; i < MaxFailedAttempts - 1; i++)
        {
            await _securityLockoutService.RecordFailure(EmailKey);
        }

        Assert.That(await _securityLockoutService.IsLockedOut(EmailKey), Is.False);

        // Advance just more than the FailedAttemptResetInterval and trigger cleanup
        _fakeTimeProvider.Advance(FailedAttemptResetInterval + TimeSpan.FromMilliseconds(1));

        // One more failure should NOT lockout because the previous 2 were stale and cleaned up.
        await _securityLockoutService.RecordFailure(EmailKey);

        Assert.That(await _securityLockoutService.IsLockedOut(EmailKey), Is.False);
    }

    [Test]
    public async Task Keys_ShouldBeCaseInsensitive()
    {
        string lowerCaseKey = EmailKey.ToLower();
        string upperCaseKey = EmailKey.ToUpper();

        await _securityLockoutService.RecordFailure(lowerCaseKey);
        for (int i = 0; i < MaxFailedAttempts - 1; i++)
        {
            await _securityLockoutService.RecordFailure(upperCaseKey);
        }

        Assert.That(await _securityLockoutService.IsLockedOut(EmailKey), Is.True);
    }
}