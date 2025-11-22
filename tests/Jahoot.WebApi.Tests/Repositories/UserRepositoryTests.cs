using Dapper;
using Jahoot.Core.Models;
using Jahoot.WebApi.Repositories;
using Microsoft.Data.Sqlite;

namespace Jahoot.WebApi.Tests.Repositories;

public class UserRepositoryTests
{
    private SqliteConnection _connection;
    private UserRepository _userRepository;

    [SetUp]
    public void Setup()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _connection.Execute("CREATE TABLE User (user_id INTEGER PRIMARY KEY AUTOINCREMENT, email TEXT, name TEXT, password_hash TEXT, created_at DATETIME, updated_at DATETIME, last_login DATETIME)");
        _userRepository = new UserRepository(_connection);
    }

    [TearDown]
    public void TearDown()
    {
        _connection.Dispose();
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
        await _connection.ExecuteAsync("INSERT INTO User (email, name, password_hash, created_at, updated_at) VALUES (@Email, @Name, @PasswordHash, @CreatedAt, @UpdatedAt)", user);

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
            UserId = 1,
            Email = "user@example.com",
            Name = "Original Name",
            PasswordHash = "original_hash",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _connection.ExecuteAsync("INSERT INTO User (user_id, email, name, password_hash, created_at, updated_at) VALUES (@UserId, @Email, @Name, @PasswordHash, @CreatedAt, @UpdatedAt)", originalUser);

        User updatedUser = new()
        {
            UserId = 1,
            Email = "updated-user@example.com",
            Name = "Updated Name",
            PasswordHash = "updated_hash",
            CreatedAt = originalUser.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow
        };

        await _userRepository.UpdateUserAsync(updatedUser);

        User? result = await _connection.QuerySingleOrDefaultAsync<User>("SELECT * FROM User WHERE user_id = @UserId", new { updatedUser.UserId });

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Email, Is.EqualTo(updatedUser.Email));
            Assert.That(result.Name, Is.EqualTo(updatedUser.Name));
            Assert.That(result.PasswordHash, Is.EqualTo(updatedUser.PasswordHash));
            Assert.That(result.LastLogin, Is.EqualTo(updatedUser.LastLogin));
        }
    }
}
