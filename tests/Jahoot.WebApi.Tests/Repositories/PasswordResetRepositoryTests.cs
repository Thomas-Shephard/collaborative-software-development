using Dapper;
using Jahoot.Core.Models;
using Jahoot.Core.Utils;
using Jahoot.WebApi.Repositories;
using Moq;

namespace Jahoot.WebApi.Tests.Repositories;

public class PasswordResetRepositoryTests : RepositoryTestBase
{
    private PasswordResetRepository _passwordResetRepository = null!;
    private Mock<IUserRepository> _mockUserRepository = null!;

    [SetUp]
    public new async Task Setup()
    {
        await base.Setup();
        _mockUserRepository = new Mock<IUserRepository>();
        _passwordResetRepository = new PasswordResetRepository(Connection, _mockUserRepository.Object);
    }

    private async Task<int> InsertUser(User user)
    {
        const string insertQuery = "INSERT INTO User (email, name, password_hash, created_at, updated_at) VALUES (@Email, @Name, @PasswordHash, @CreatedAt, @UpdatedAt); SELECT LAST_INSERT_ID();";
        return await Connection.QuerySingleAsync<int>(insertQuery, user);
    }

    [Test]
    public async Task CreateTokenAsync_SuccessfulTokenCreation_InvalidatesOldInsertsNewAndCommits()
    {
        User user = new User { UserId = 1, Email = "test@example.com", Name = "Test User", PasswordHash = "hash", Roles = [], CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        int userId = await InsertUser(user);

        // Simulate an existing token that should be invalidated
        await Connection.ExecuteAsync("INSERT INTO PasswordResetToken (user_id, token_hash, expiration, is_used, is_revoked, created_at) VALUES (@UserId, @TokenHash, @Expiration, @IsUsed, @IsRevoked, @CreatedAt)",
            new
            {
                UserId = userId,
                TokenHash = "old_token_hash",
                Expiration = DateTime.UtcNow.AddDays(1),
                IsUsed = false,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            });

        const string newToken = "new_token_plain";
        string newTokenHash = PasswordUtils.HashPasswordWithSalt(newToken);

        await _passwordResetRepository.CreateTokenAsync(userId, newTokenHash);

        // Verify old token is invalidated
        PasswordResetToken? oldToken = await Connection.QuerySingleOrDefaultAsync<PasswordResetToken>("SELECT * FROM PasswordResetToken WHERE token_hash = @TokenHash", new { TokenHash = "old_token_hash" });
        Assert.That(oldToken, Is.Not.Null);
        Assert.That(oldToken.IsRevoked, Is.True);

        // Verify new token is inserted
        PasswordResetToken? newlyInsertedToken = await Connection.QuerySingleOrDefaultAsync<PasswordResetToken>("SELECT * FROM PasswordResetToken WHERE token_hash = @TokenHash", new { TokenHash = newTokenHash });
        Assert.That(newlyInsertedToken, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(newlyInsertedToken.UserId, Is.EqualTo(userId));
            Assert.That(newlyInsertedToken.TokenHash, Is.EqualTo(newTokenHash));
            Assert.That(newlyInsertedToken.IsUsed, Is.False);
            Assert.That(newlyInsertedToken.IsRevoked, Is.False);
        }
    }

    [Test]
    public async Task GetPasswordResetTokenByEmail_UserNotFound_ReturnsNull()
    {
        _mockUserRepository.Setup(r => r.GetUserByEmailAsync("nonexistent@example.com")).ReturnsAsync((User?)null);

        PasswordResetToken? result = await _passwordResetRepository.GetPasswordResetTokenByEmail("nonexistent@example.com");

        Assert.That(result, Is.Null);
        _mockUserRepository.Verify(r => r.GetUserByEmailAsync("nonexistent@example.com"), Times.Once);
    }

    [Test]
    public async Task GetPasswordResetTokenByEmail_TokenFound_ReturnsToken()
    {
        User user = new() { UserId = 1, Email = "test@example.com", Name = "Test User", PasswordHash = "hash", Roles = [], CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        int userId = await InsertUser(user);
        _mockUserRepository.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);

        string tokenHash = PasswordUtils.HashPasswordWithSalt("valid_token");
        await Connection.ExecuteAsync("INSERT INTO PasswordResetToken (user_id, token_hash, expiration, is_used, is_revoked, created_at) VALUES (@UserId, @TokenHash, @Expiration, @IsUsed, @IsRevoked, @CreatedAt)",
            new
            {
                UserId = userId,
                TokenHash = tokenHash,
                Expiration = DateTime.UtcNow.AddDays(1),
                IsUsed = false,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            });

        PasswordResetToken? result = await _passwordResetRepository.GetPasswordResetTokenByEmail(user.Email);

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.UserId, Is.EqualTo(userId));
            Assert.That(result.TokenHash, Is.EqualTo(tokenHash));
            Assert.That(result.IsUsed, Is.False);
            Assert.That(result.IsRevoked, Is.False);
        }
        _mockUserRepository.Verify(r => r.GetUserByEmailAsync(user.Email), Times.Once);
    }

    [Test]
    public async Task GetPasswordResetTokenByEmail_TokenNotFoundForUser_ReturnsNull()
    {
        User user = new() { UserId = 1, Email = "test@example.com", Name = "Test User", PasswordHash = "hash", Roles = [], CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        await InsertUser(user);
        _mockUserRepository.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);

        PasswordResetToken? result = await _passwordResetRepository.GetPasswordResetTokenByEmail(user.Email);

        Assert.That(result, Is.Null);
        _mockUserRepository.Verify(r => r.GetUserByEmailAsync(user.Email), Times.Once);
    }

    [Test]
    public async Task UpdatePasswordResetTokenAsync_SuccessfulUpdate_UpdatesToken()
    {
        User user = new() { UserId = 1, Email = "test@example.com", Name = "Test User", PasswordHash = "hash", Roles = [], CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        int userId = await InsertUser(user);

        string tokenHash = PasswordUtils.HashPasswordWithSalt("valid_token");
        await Connection.ExecuteAsync("INSERT INTO PasswordResetToken (user_id, token_hash, expiration, is_used, is_revoked, created_at) VALUES (@UserId, @TokenHash, @Expiration, @IsUsed, @IsRevoked, @CreatedAt)",
            new
            {
                UserId = userId,
                TokenHash = tokenHash,
                Expiration = DateTime.UtcNow.AddDays(1),
                IsUsed = false,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            });

        PasswordResetToken? originalToken = await Connection.QuerySingleOrDefaultAsync<PasswordResetToken>("SELECT * FROM PasswordResetToken WHERE token_hash = @TokenHash", new { TokenHash = tokenHash });
        Assert.That(originalToken, Is.Not.Null);
        Assert.That(originalToken.IsUsed, Is.False);

        originalToken.IsUsed = true;

        await _passwordResetRepository.UpdatePasswordResetTokenAsync(originalToken);

        PasswordResetToken? updatedToken = await Connection.QuerySingleOrDefaultAsync<PasswordResetToken>("SELECT * FROM PasswordResetToken WHERE token_id = @TokenId", new { originalToken.TokenId });
        Assert.That(updatedToken, Is.Not.Null);
        Assert.That(updatedToken.IsUsed, Is.True);
    }

    [Test]
    public async Task CreateTokenAsync_InvalidUserId_RollsBackTransactionAndThrows()
    {
        const int nonExistentUserId = 9999;
        const string token = "new_token_plain";
        string newTokenHash = PasswordUtils.HashPasswordWithSalt(token);

        User user = new() { UserId = 1, Email = "valid@example.com", Name = "Valid User", PasswordHash = "hash", Roles = [], CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        int validUserId = await InsertUser(user);
        await Connection.ExecuteAsync("INSERT INTO PasswordResetToken (user_id, token_hash, expiration, is_used, is_revoked, created_at) VALUES (@UserId, @TokenHash, @Expiration, @IsUsed, @IsRevoked, @CreatedAt)",
            new
            {
                UserId = validUserId,
                TokenHash = "old_token_hash",
                Expiration = DateTime.UtcNow.AddDays(1),
                IsUsed = false,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            });

        // Expecting an exception due to foreign key constraint violation
        Assert.That(async () => await _passwordResetRepository.CreateTokenAsync(nonExistentUserId, newTokenHash), Throws.Exception);

        // Assert that the old token for the valid user was NOT invalidated (rollback occurred)
        PasswordResetToken? oldToken = await Connection.QuerySingleOrDefaultAsync<PasswordResetToken>("SELECT * FROM PasswordResetToken WHERE user_id = @UserId AND token_hash = @TokenHash", new { UserId = validUserId, TokenHash = "old_token_hash" });
        Assert.That(oldToken, Is.Not.Null);
        Assert.That(oldToken.IsRevoked, Is.False);

        // Assert that no new token was inserted for the non-existent user (rollback occurred)
        PasswordResetToken? newTokenInDb = await Connection.QuerySingleOrDefaultAsync<PasswordResetToken>("SELECT * FROM PasswordResetToken WHERE user_id = @UserId AND token_hash = @TokenHash", new { UserId = nonExistentUserId, TokenHash = newTokenHash });
        Assert.That(newTokenInDb, Is.Null);
    }
}
