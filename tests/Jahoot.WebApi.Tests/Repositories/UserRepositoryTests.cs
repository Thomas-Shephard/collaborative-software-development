using Dapper;
using Jahoot.Core.Models;
using Jahoot.WebApi.Repositories;

namespace Jahoot.WebApi.Tests.Repositories;

public class UserRepositoryTests : RepositoryTestBase
{
    private UserRepository _userRepository = null!;

    [SetUp]
    public new void Setup()
    {
        _userRepository = new UserRepository(Connection);
    }

    [Test]
    public async Task GetUserByEmailAsync_UserExists_ReturnsUser()
    {
        User user = new()
        {
            Email = "test@example.com",
            Name = "Test User",
            PasswordHash = "password_hash",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await Connection.ExecuteAsync("INSERT INTO User (email, name, password_hash, created_at, updated_at) VALUES (@Email, @Name, @PasswordHash, @CreatedAt, @UpdatedAt)", user);

        User? result = await _userRepository.GetUserByEmailAsync("test@example.com");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Email, Is.EqualTo(user.Email));
    }

    [Test]
    public async Task GetUserByEmailAsync_UserDoesNotExist_ReturnsNull()
    {
        User? result = await _userRepository.GetUserByEmailAsync("nonexistent@example.com");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateUserAsync_UserExists_UpdatesUser()
    {
        User originalUser = new()
        {
            Email = "user@example.com",
            Name = "Original Name",
            PasswordHash = "original_hash",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        const string insertQuery = "INSERT INTO User (email, name, password_hash, created_at, updated_at) VALUES (@Email, @Name, @PasswordHash, @CreatedAt, @UpdatedAt); SELECT LAST_INSERT_ID();";
        int userId = await Connection.QuerySingleAsync<int>(insertQuery, originalUser);

        User updatedUser = new()
        {
            UserId = userId,
            Email = "updated-user@example.com",
            Name = "Updated Name",
            PasswordHash = "updated_hash",
            CreatedAt = originalUser.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow
        };

        await _userRepository.UpdateUserAsync(updatedUser);

        User? result = await Connection.QuerySingleOrDefaultAsync<User>("SELECT * FROM User WHERE user_id = @UserId", new { updatedUser.UserId });

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Email, Is.EqualTo(updatedUser.Email));
            Assert.That(result.Name, Is.EqualTo(updatedUser.Name));
            Assert.That(result.PasswordHash, Is.EqualTo(updatedUser.PasswordHash));
            Assert.That(result.LastLogin, Is.EqualTo(updatedUser.LastLogin).Within(TimeSpan.FromSeconds(1)));
        }
    }
}
