using Jahoot.WebApi.Services;
using Jahoot.WebApi.Settings;
using Microsoft.Extensions.Time.Testing;

namespace Jahoot.WebApi.Tests.Services;

public class LoginAttemptServiceTests
{
    private const int MaxFailedLoginAttempts = 3;
    private const string Email = "test@example.com";
    private const string IpAddress = "127.0.0.1";
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMilliseconds(10);
    private static readonly TimeSpan CleanupInterval = LockoutDuration;
    private static readonly TimeSpan FailedAttemptResetInterval = LockoutDuration;
    private FakeTimeProvider _fakeTimeProvider;

    private LoginAttemptService _loginAttemptService;

    [SetUp]
    public void SetUp()
    {
        _fakeTimeProvider = new FakeTimeProvider();
        LoginAttemptSettings loginAttemptSettings = new()
        {
            MaxFailedAttempts = MaxFailedLoginAttempts,
            InitialLockoutDuration = LockoutDuration,
            IncrementalLockoutDuration = LockoutDuration,
            CleanupInterval = CleanupInterval,
            FailedAttemptResetInterval = FailedAttemptResetInterval
        };
        _loginAttemptService = new LoginAttemptService(loginAttemptSettings, _fakeTimeProvider);
    }

    [TearDown]
    public void TearDown()
    {
        _loginAttemptService.Dispose();
    }

    [Test]
    public async Task IsLockedOut_ShouldReturnFalse_WhenNotLockedOut()
    {
        bool result = await _loginAttemptService.IsLockedOut(Email, IpAddress);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task RecordFailedLoginAttempt_ShouldLockout_AfterThreeAttempts()
    {
        for (int i = 0; i < MaxFailedLoginAttempts; i++)
        {
            await _loginAttemptService.RecordFailedLoginAttempt(Email, IpAddress);
        }

        bool result = await _loginAttemptService.IsLockedOut(Email, IpAddress);
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsLockedOut_ShouldReturnFalse_AfterLockoutExpires()
    {
        for (int i = 0; i < MaxFailedLoginAttempts; i++)
        {
            await _loginAttemptService.RecordFailedLoginAttempt(Email, IpAddress);
        }

        bool isLockedOut = await _loginAttemptService.IsLockedOut(Email, IpAddress);
        Assert.That(isLockedOut, Is.True);

        _fakeTimeProvider.Advance(LockoutDuration);

        bool isStillLockedOut = await _loginAttemptService.IsLockedOut(Email, IpAddress);
        Assert.That(isStillLockedOut, Is.False);
    }

    [Test]
    public async Task ResetFailedLoginAttempts_ShouldResetAttempts()
    {
        for (int i = 0; i < MaxFailedLoginAttempts; i++)
        {
            await _loginAttemptService.RecordFailedLoginAttempt(Email, IpAddress);
        }

        bool isLockedOut = await _loginAttemptService.IsLockedOut(Email, IpAddress);
        Assert.That(isLockedOut, Is.True);

        _fakeTimeProvider.Advance(LockoutDuration);

        await _loginAttemptService.ResetFailedLoginAttempts(Email, IpAddress);
        await _loginAttemptService.RecordFailedLoginAttempt(Email, IpAddress);

        bool isStillLockedOut = await _loginAttemptService.IsLockedOut(Email, IpAddress);
        Assert.That(isStillLockedOut, Is.False);
    }

    [Test]
    public async Task RecordFailedLoginAttempt_ShouldIncreaseLockoutDuration_WhenFailingWhileLockedOut()
    {
        for (int i = 0; i < MaxFailedLoginAttempts; i++)
        {
            await _loginAttemptService.RecordFailedLoginAttempt(Email, IpAddress);
        }

        bool isLockedOut = await _loginAttemptService.IsLockedOut(Email, IpAddress);
        Assert.That(isLockedOut, Is.True);

        _fakeTimeProvider.Advance(LockoutDuration);

        isLockedOut = await _loginAttemptService.IsLockedOut(Email, IpAddress);
        Assert.That(isLockedOut, Is.False);

        await _loginAttemptService.RecordFailedLoginAttempt(Email, IpAddress);

        isLockedOut = await _loginAttemptService.IsLockedOut(Email, IpAddress);
        Assert.That(isLockedOut, Is.True);

        _fakeTimeProvider.Advance(LockoutDuration);

        isLockedOut = await _loginAttemptService.IsLockedOut(Email, IpAddress);
        Assert.That(isLockedOut, Is.True);

        _fakeTimeProvider.Advance(LockoutDuration);

        isLockedOut = await _loginAttemptService.IsLockedOut(Email, IpAddress);
        Assert.That(isLockedOut, Is.False);
    }

    [Test]
    public async Task CleanupExpiredLockouts_ShouldRemoveExpiredLockouts()
    {
        for (int i = 0; i < MaxFailedLoginAttempts; i++)
        {
            await _loginAttemptService.RecordFailedLoginAttempt(Email, IpAddress);
        }

        Assert.That(await _loginAttemptService.IsLockedOut(Email, IpAddress), Is.True);

        _fakeTimeProvider.Advance(LockoutDuration);
        Assert.That(await _loginAttemptService.IsLockedOut(Email, IpAddress), Is.False);

        _fakeTimeProvider.Advance(CleanupInterval);

        await _loginAttemptService.RecordFailedLoginAttempt(Email, IpAddress);
        Assert.That(await _loginAttemptService.IsLockedOut(Email, IpAddress), Is.False);

        for (int i = 0; i < MaxFailedLoginAttempts - 1; i++)
        {
            await _loginAttemptService.RecordFailedLoginAttempt(Email, IpAddress);
        }

        Assert.That(await _loginAttemptService.IsLockedOut(Email, IpAddress), Is.True);
    }

    [Test]
    public async Task LoginAttempts_ShouldBeCaseInsensitiveForEmail()
    {
        string lowerCaseEmail = Email.ToLower();
        string upperCaseEmail = Email.ToUpper();

        await _loginAttemptService.RecordFailedLoginAttempt(lowerCaseEmail, $"{IpAddress}-1");
        for (int i = 0; i < MaxFailedLoginAttempts - 1; i++)
        {
            await _loginAttemptService.RecordFailedLoginAttempt(upperCaseEmail, $"{IpAddress}-2");
        }

        Assert.That(await _loginAttemptService.IsLockedOut(Email, IpAddress), Is.True);
    }

    [Test]
    public async Task CleanupExpiredStaleAttempts_ShouldRemoveAttemptsWithoutLockout()
    {
        for (int i = 0; i < MaxFailedLoginAttempts - 1; i++)
        {
            await _loginAttemptService.RecordFailedLoginAttempt(Email, IpAddress);
        }

        Assert.That(await _loginAttemptService.IsLockedOut(Email, IpAddress), Is.False);

        // Advance just more than the FailedAttemptResetInterval to ensure the timer clean-up occurs first
        _fakeTimeProvider.Advance(FailedAttemptResetInterval + TimeSpan.FromMilliseconds(1));

        await _loginAttemptService.RecordFailedLoginAttempt(Email, IpAddress);

        Assert.That(await _loginAttemptService.IsLockedOut(Email, IpAddress), Is.False);
    }
}
