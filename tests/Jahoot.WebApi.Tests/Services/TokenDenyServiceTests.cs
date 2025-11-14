using Jahoot.WebApi.Services;

namespace Jahoot.WebApi.Tests.Services;

public class TokenDenyServiceTests
{
    private const string JtiValid = "test_jti";
    private const string JtiExpired = "test_jti_expired";

    private TokenDenyService _tokenDenyService;

    [SetUp]
    public void Setup()
    {
        _tokenDenyService = new TokenDenyService();
    }

    [TearDown]
    public void Teardown()
    {
        _tokenDenyService.Dispose();
    }

    [Test]
    public async Task IsDeniedAsync_ReturnsTrueForDeniedToken()
    {
        DateTime expiry = DateTime.UtcNow.AddHours(1);

        await _tokenDenyService.DenyAsync(JtiValid, expiry);

        Assert.That(await _tokenDenyService.IsDeniedAsync(JtiValid), Is.True);
    }

    [Test]
    public async Task IsDeniedAsync_ReturnsFalseForNonDeniedToken()
    {
        Assert.That(await _tokenDenyService.IsDeniedAsync(JtiValid), Is.False);
    }

    [Test]
    public async Task CleanupExpiredTokens_RemovesExpiredTokens()
    {
        TimeSpan shortTimeSpan = TimeSpan.FromMilliseconds(10);

        using TokenDenyService shortIntervalService = new(shortTimeSpan);
        DateTime expiryExpired = DateTime.UtcNow.Subtract(shortTimeSpan);
        DateTime expiryValid = DateTime.UtcNow.AddHours(1);

        await shortIntervalService.DenyAsync(JtiExpired, expiryExpired);
        await shortIntervalService.DenyAsync(JtiValid, expiryValid);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(await shortIntervalService.IsDeniedAsync(JtiExpired), Is.True);
            Assert.That(await shortIntervalService.IsDeniedAsync(JtiValid), Is.True);
        }

        await Task.Delay(5 * shortTimeSpan);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(await shortIntervalService.IsDeniedAsync(JtiExpired), Is.False);
            Assert.That(await shortIntervalService.IsDeniedAsync(JtiValid), Is.True);
        }
    }
}
