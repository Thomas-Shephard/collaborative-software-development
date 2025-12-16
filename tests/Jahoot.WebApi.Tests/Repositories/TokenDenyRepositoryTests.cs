using Dapper;
using Jahoot.WebApi.Repositories;

namespace Jahoot.WebApi.Tests.Repositories;

public class TokenDenyRepositoryTests : RepositoryTestBase
{
    private TokenDenyRepository _tokenDenyRepository;

    [SetUp]
    public new void Setup()
    {
        _tokenDenyRepository = new TokenDenyRepository(Connection);
    }

    [Test]
    public async Task DenyTokenAsync_AddsTokenToDeniedList()
    {
        string jti = Guid.NewGuid().ToString();
        DateTime expiresAt = DateTime.UtcNow.AddHours(1);

        await _tokenDenyRepository.DenyTokenAsync(jti, expiresAt);

        string? resultJti = await Connection.QuerySingleOrDefaultAsync<string>("SELECT jti FROM DeniedToken WHERE jti = @Jti", new { Jti = jti });
        Assert.That(resultJti, Is.EqualTo(jti));
    }

    [Test]
    public async Task DenyTokenAsync_DoesNotAddDuplicateToken()
    {
        string jti = Guid.NewGuid().ToString();
        DateTime expiresAt = DateTime.UtcNow.AddHours(1);

        await _tokenDenyRepository.DenyTokenAsync(jti, expiresAt);
        await _tokenDenyRepository.DenyTokenAsync(jti, expiresAt.AddHours(2));

        int count = await Connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM DeniedToken WHERE jti = @Jti", new { Jti = jti });
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public async Task DeleteExpiredTokensAsync_RemovesOnlyExpiredTokens()
    {
        string jti1 = Guid.NewGuid().ToString();
        DateTime expiresAt1 = DateTime.UtcNow.AddMinutes(-10); // Expired
        await _tokenDenyRepository.DenyTokenAsync(jti1, expiresAt1);

        string jti2 = Guid.NewGuid().ToString();
        DateTime expiresAt2 = DateTime.UtcNow.AddMinutes(10); // Not expired
        await _tokenDenyRepository.DenyTokenAsync(jti2, expiresAt2);

        await _tokenDenyRepository.DeleteExpiredTokensAsync(DateTime.UtcNow);

        string[] activeJtis = (await Connection.QueryAsync<string>("SELECT jti FROM DeniedToken")).ToArray();
        Assert.That(activeJtis, Has.One.EqualTo(jti2));
        Assert.That(activeJtis, Has.None.EqualTo(jti1));
    }

    [Test]
    public async Task GetActiveDeniedTokensAsync_ReturnsOnlyActiveTokens()
    {
        string jti1 = Guid.NewGuid().ToString();
        DateTime expiresAt1 = DateTime.UtcNow.AddMinutes(-10); // Expired
        await _tokenDenyRepository.DenyTokenAsync(jti1, expiresAt1);

        string jti2 = Guid.NewGuid().ToString();
        DateTime expiresAt2 = DateTime.UtcNow.AddMinutes(10); // Not expired
        await _tokenDenyRepository.DenyTokenAsync(jti2, expiresAt2);

        string[] activeJtis = (await _tokenDenyRepository.GetActiveDeniedTokensAsync(DateTime.UtcNow)).Select(t => t.Jti).ToArray();
        Assert.That(activeJtis, Has.One.EqualTo(jti2));
        Assert.That(activeJtis, Has.None.EqualTo(jti1));
    }

    [Test]
    public async Task GetActiveDeniedTokensAsync_NoActiveTokens_ReturnsEmpty()
    {
        string jti1 = Guid.NewGuid().ToString();
        DateTime expiresAt1 = DateTime.UtcNow.AddMinutes(-10); // Expired
        await _tokenDenyRepository.DenyTokenAsync(jti1, expiresAt1);

        IEnumerable<(string Jti, DateTime ExpiresAt)> activeTokens = await _tokenDenyRepository.GetActiveDeniedTokensAsync(DateTime.UtcNow);

        Assert.That(activeTokens, Is.Empty);
    }
}
