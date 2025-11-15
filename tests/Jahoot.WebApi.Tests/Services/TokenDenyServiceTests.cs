using Jahoot.WebApi.Services;
using Jahoot.WebApi.Settings;
using Microsoft.Extensions.Time.Testing;

namespace Jahoot.WebApi.Tests.Services;

public class TokenDenyServiceTests
{
    private const string JtiValid = "test_jti";
    private const string JtiExpired = "test_jti_expired";
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMilliseconds(10);
    private FakeTimeProvider _fakeTimeProvider;

    private TokenDenyService _tokenDenyService;

    [SetUp]
    public void Setup()
    {
        _fakeTimeProvider = new FakeTimeProvider();
        TokenDenySettings tokenDenySettings = new()
        {
            CleanupInterval = CleanupInterval
        };
        _tokenDenyService = new TokenDenyService(tokenDenySettings, _fakeTimeProvider);
    }

    [TearDown]
    public void Teardown()
    {
        _tokenDenyService.Dispose();
    }

    [Test]
    public async Task IsDeniedAsync_ReturnsTrueForDeniedToken()
    {
        DateTime expiry = _fakeTimeProvider.GetUtcNow().AddHours(1).UtcDateTime;

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
        DateTime expiryExpired = _fakeTimeProvider.GetUtcNow().AddHours(-1).UtcDateTime;
        DateTime expiryValid = _fakeTimeProvider.GetUtcNow().AddHours(1).UtcDateTime;

        await _tokenDenyService.DenyAsync(JtiExpired, expiryExpired);
        await _tokenDenyService.DenyAsync(JtiValid, expiryValid);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(await _tokenDenyService.IsDeniedAsync(JtiExpired), Is.True);
            Assert.That(await _tokenDenyService.IsDeniedAsync(JtiValid), Is.True);
        }

        _fakeTimeProvider.Advance(CleanupInterval);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(await _tokenDenyService.IsDeniedAsync(JtiExpired), Is.False);
            Assert.That(await _tokenDenyService.IsDeniedAsync(JtiValid), Is.True);
        }
    }
}
