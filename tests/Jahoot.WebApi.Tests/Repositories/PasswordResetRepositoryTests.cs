using Jahoot.Core.Models;
using Jahoot.WebApi.Repositories;

namespace Jahoot.WebApi.Tests.Repositories;

public class PasswordResetRepositoryTests : RepositoryTestBase
{
    private PasswordResetRepository _passwordResetRepository;

    [SetUp]
    public new void Setup()
    {
        _passwordResetRepository = new PasswordResetRepository(Connection);
    }

    [Test]
    public async Task CreateTokenAsync_UserExists_CreatesToken()
    {
        User user = new()
        {
            Email = "test@example.com",
            Name = "Test User",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Roles = []
        };
        int userId = await InsertUser(user);
        const string token = "123456";

        await _passwordResetRepository.CreateTokenAsync(userId, token);

        PasswordResetToken? passwordResetToken = await _passwordResetRepository.GetTokenByTokenAsync(token);

        Assert.That(passwordResetToken, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(passwordResetToken.UserId, Is.EqualTo(userId));
            Assert.That(passwordResetToken.Token, Is.EqualTo(token));
            Assert.That(passwordResetToken.Expiration, Is.EqualTo(DateTime.UtcNow + TimeSpan.FromMinutes(15)).Within(TimeSpan.FromSeconds(1)));
            Assert.That(passwordResetToken.IsUsed, Is.False);
            Assert.That(passwordResetToken.CreatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(1)));
        }
    }
}
