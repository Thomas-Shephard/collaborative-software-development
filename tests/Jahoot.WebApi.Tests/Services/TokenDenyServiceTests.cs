using Jahoot.WebApi.Repositories;
using Jahoot.WebApi.Services;
using Jahoot.WebApi.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Jahoot.WebApi.Tests.Services;

public class TokenDenyServiceTests
{
    private const string JtiValid = "test_jti";
    private const string JtiExpired = "test_jti_expired";
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMilliseconds(10);
    private FakeTimeProvider _fakeTimeProvider;
    private Mock<IServiceScopeFactory> _mockScopeFactory;
    private Mock<IServiceScope> _mockScope;
    private Mock<IServiceProvider> _mockServiceProvider;
    private Mock<ITokenDenyRepository> _mockTokenDenyRepository;

    private TokenDenyService _tokenDenyService;

    [SetUp]
    public void Setup()
    {
        _fakeTimeProvider = new FakeTimeProvider();
        TokenDenySettings tokenDenySettings = new()
        {
            CleanupInterval = CleanupInterval
        };

        _mockTokenDenyRepository = new Mock<ITokenDenyRepository>();

        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockServiceProvider.Setup(x => x.GetService(typeof(ITokenDenyRepository))).Returns(_mockTokenDenyRepository.Object);

        _mockScope = new Mock<IServiceScope>();
        _mockScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);

        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);

        _mockTokenDenyRepository.Setup(x => x.GetActiveDeniedTokensAsync(It.IsAny<DateTime>())).ReturnsAsync([]);

        _tokenDenyService = new TokenDenyService(tokenDenySettings, _fakeTimeProvider, _mockScopeFactory.Object);
    }

    [TearDown]
    public void Teardown()
    {
        _tokenDenyService.Dispose();
    }

    [Test]
    public async Task Constructor_LoadsDeniedTokensFromDb()
    {
        List<(string Jti, DateTime Expiry)> initialTokens = [("loaded_jti", DateTime.UtcNow.AddHours(1))];

        _mockTokenDenyRepository.Setup(x => x.GetActiveDeniedTokensAsync(It.IsAny<DateTime>())).ReturnsAsync(initialTokens);

        TokenDenySettings tokenDenySettings = new() { CleanupInterval = CleanupInterval };

        TokenDenyService service = new(tokenDenySettings, _fakeTimeProvider, _mockScopeFactory.Object);

        Assert.That(await service.IsDeniedAsync("loaded_jti"), Is.True);
        _mockTokenDenyRepository.Verify(x => x.GetActiveDeniedTokensAsync(It.IsAny<DateTime>()), Times.AtLeastOnce);
    }

    [Test]
    public async Task DenyAsync_AddsToMemoryAndRepository()
    {
        DateTime expiry = _fakeTimeProvider.GetUtcNow().AddHours(1).UtcDateTime;

        await _tokenDenyService.DenyAsync(JtiValid, expiry);

        Assert.That(await _tokenDenyService.IsDeniedAsync(JtiValid), Is.True);
        _mockTokenDenyRepository.Verify(x => x.DenyTokenAsync(JtiValid, expiry), Times.Once);
    }

    [Test]
    public async Task IsDeniedAsync_ReturnsFalseForNonDeniedToken()
    {
        Assert.That(await _tokenDenyService.IsDeniedAsync(JtiValid), Is.False);
    }

    [Test]
    public async Task CleanupExpiredTokens_RemovesExpiredTokensFromMemoryAndRepository()
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

        _mockTokenDenyRepository.Verify(x => x.DeleteExpiredTokensAsync(It.IsAny<DateTime>()), Times.AtLeastOnce);
    }

    [Test]
    public void CleanupExpiredTokens_SwallowsException_WhenRepositoryThrows()
    {
        _mockTokenDenyRepository
            .Setup(x => x.DeleteExpiredTokensAsync(It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        Assert.DoesNotThrow(() => _fakeTimeProvider.Advance(CleanupInterval));
        _mockTokenDenyRepository.Verify(x => x.DeleteExpiredTokensAsync(It.IsAny<DateTime>()), Times.AtLeastOnce);
    }

    [Test]
    public async Task DenyAsync_DoesNotAddToMemory_WhenRepositoryThrows()
    {
        DateTime expiry = _fakeTimeProvider.GetUtcNow().AddHours(1).UtcDateTime;
        _mockTokenDenyRepository.Setup(x => x.DenyTokenAsync(It.IsAny<string>(), It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("DB Error"));

        Assert.ThrowsAsync<Exception>(() => _tokenDenyService.DenyAsync(JtiValid, expiry));
        Assert.That(await _tokenDenyService.IsDeniedAsync(JtiValid), Is.False);
    }
}
