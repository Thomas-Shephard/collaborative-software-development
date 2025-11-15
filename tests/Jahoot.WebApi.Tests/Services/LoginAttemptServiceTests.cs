using Jahoot.WebApi.Services;

namespace Jahoot.WebApi.Tests.Services;

[TestFixture]
public class LoginAttemptServiceTests
{
    [SetUp]
    public void SetUp()
    {
        _loginAttemptService = new LoginAttemptService(TimeSpan.FromMilliseconds(100));
    }

    [TearDown]
    public void TearDown()
    {
        _loginAttemptService.Dispose();
    }

    private LoginAttemptService _loginAttemptService = null!;

    [Test]
    public async Task IsLockedOut_ShouldReturnFalse_WhenNotLockedOut()
    {
        // Act
        bool result = await _loginAttemptService.IsLockedOut("testuser", "127.0.0.1");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task RecordFailedLoginAttempt_ShouldLockout_AfterThreeAttempts()
    {
        // Arrange
        const string username = "testuser";
        const string ipAddress = "127.0.0.1";

        // Act
        await _loginAttemptService.RecordFailedLoginAttempt(username, ipAddress);
        await _loginAttemptService.RecordFailedLoginAttempt(username, ipAddress);
        await _loginAttemptService.RecordFailedLoginAttempt(username, ipAddress);

        // Assert
        bool result = await _loginAttemptService.IsLockedOut(username, ipAddress);
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsLockedOut_ShouldReturnFalse_AfterLockoutExpires()
    {
        // Arrange
        const string username = "testuser";
        const string ipAddress = "127.0.0.1";
        _loginAttemptService = new LoginAttemptService(TimeSpan.FromMilliseconds(10));


        // Act
        await _loginAttemptService.RecordFailedLoginAttempt(username, ipAddress);
        await _loginAttemptService.RecordFailedLoginAttempt(username, ipAddress);
        await _loginAttemptService.RecordFailedLoginAttempt(username, ipAddress);

        // Assert
        bool isLockedOut = await _loginAttemptService.IsLockedOut(username, ipAddress);
        Assert.That(isLockedOut, Is.True);

        await Task.Delay(20); // Wait for lockout to expire

        bool isStillLockedOut = await _loginAttemptService.IsLockedOut(username, ipAddress);
        Assert.That(isStillLockedOut, Is.False);
    }

    [Test]
    public void ResetFailedLoginAttempts_ShouldResetAttempts()
    {
        // Arrange
        const string username = "testuser";
        const string ipAddress = "127.0.0.1";
        _loginAttemptService.RecordFailedLoginAttempt(username, ipAddress);
        _loginAttemptService.RecordFailedLoginAttempt(username, ipAddress);

        // Act
        _loginAttemptService.ResetFailedLoginAttempts(username, ipAddress);
        _loginAttemptService.RecordFailedLoginAttempt(username, ipAddress);


        // Assert
        bool result = _loginAttemptService.IsLockedOut(username, ipAddress).Result;
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task RecordFailedLoginAttempt_ShouldIncreaseLockoutDurationLinearly()
    {
        // Arrange
        const string username = "testuser";
        const string ipAddress = "127.0.0.1";
        _loginAttemptService = new LoginAttemptService(TimeSpan.FromMilliseconds(10));

        // Act & Assert
        // First lockout (15s)
        await _loginAttemptService.RecordFailedLoginAttempt(username, ipAddress);
        await _loginAttemptService.RecordFailedLoginAttempt(username, ipAddress);
        await _loginAttemptService.RecordFailedLoginAttempt(username, ipAddress);
        Assert.That(await _loginAttemptService.IsLockedOut(username, ipAddress), Is.True);
        await Task.Delay(20);
        Assert.That(await _loginAttemptService.IsLockedOut(username, ipAddress), Is.False);

        // Second lockout (20s)
        await _loginAttemptService.RecordFailedLoginAttempt(username, ipAddress);
        await _loginAttemptService.RecordFailedLoginAttempt(username, ipAddress);
        await _loginAttemptService.RecordFailedLoginAttempt(username, ipAddress);
        Assert.That(await _loginAttemptService.IsLockedOut(username, ipAddress), Is.True);
        await Task.Delay(20);
        Assert.That(await _loginAttemptService.IsLockedOut(username, ipAddress), Is.False);
    }
}
