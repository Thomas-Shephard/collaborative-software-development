using Jahoot.Core.Models;

namespace Jahoot.Core.Tests.Models;

public class PasswordResetTokenTests
{
    private const int TestTokenId = 1;
    private const int TestUserId = 101;
    private const string TestTokenHash = "some_hashed_token";
    private static readonly DateTime TestExpiration = DateTime.UtcNow.AddHours(1);
    private static readonly DateTime TestCreatedAt = DateTime.UtcNow.AddMinutes(-30);

    [Test]
    public void PasswordResetToken_CanBeCreated_WithValidProperties()
    {
        PasswordResetToken token = new()
        {
            TokenId = TestTokenId,
            UserId = TestUserId,
            TokenHash = TestTokenHash,
            Expiration = TestExpiration,
            IsUsed = false,
            IsRevoked = false,
            CreatedAt = TestCreatedAt
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(token.TokenId, Is.EqualTo(TestTokenId));
            Assert.That(token.UserId, Is.EqualTo(TestUserId));
            Assert.That(token.TokenHash, Is.EqualTo(TestTokenHash));
            Assert.That(token.Expiration, Is.EqualTo(TestExpiration));
            Assert.That(token.IsUsed, Is.False);
            Assert.That(token.IsRevoked, Is.False);
            Assert.That(token.CreatedAt, Is.EqualTo(TestCreatedAt));
        }
    }

    [Test]
    public void PasswordResetToken_IsUsed_CanBeSet()
    {
        PasswordResetToken token = new()
        {
            TokenId = TestTokenId,
            UserId = TestUserId,
            TokenHash = TestTokenHash,
            Expiration = TestExpiration,
            IsUsed = false,
            IsRevoked = false,
            CreatedAt = TestCreatedAt
        };

        token.IsUsed = true;

        Assert.That(token.IsUsed, Is.True);
    }
}
