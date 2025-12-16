using Jahoot.Core.Models;
using System.ComponentModel;

namespace Jahoot.Core.Tests.Models;

public class UserTests
{
    private const int UserId = 1;
    private const string UserInitEmail = "test@example.com";
    private const string UserInitName = "Initial Name";
    private const string UserInitPasswordHash = "initial_hash";
    private static readonly List<Role> UserInitRoles = [Role.Admin, Role.Lecturer];

    [Test]
    public void User_CanBeCreated_WithValidProperties()
    {
        DateTime now = DateTime.UtcNow;
        User user = new()
        {
            UserId = UserId,
            Email = UserInitEmail,
            Name = UserInitName,
            PasswordHash = UserInitPasswordHash,
            Roles = UserInitRoles,
            LastLogin = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(user.UserId, Is.EqualTo(UserId));
            Assert.That(user.Email, Is.EqualTo(UserInitEmail));
            Assert.That(user.Name, Is.EqualTo(UserInitName));
            Assert.That(user.PasswordHash, Is.EqualTo(UserInitPasswordHash));
            Assert.That(user.Roles, Is.EqualTo(UserInitRoles));
            Assert.That(user.LastLogin, Is.EqualTo(now));
            Assert.That(user.CreatedAt, Is.EqualTo(now));
            Assert.That(user.UpdatedAt, Is.EqualTo(now));
        }
    }

    [Test]
    public void User_Properties_CanBeSet()
    {
        User user = new()
        {
            Email = UserInitEmail,
            Name = UserInitName,
            PasswordHash = UserInitPasswordHash,
            Roles = UserInitRoles
        };

        const string newEmail = "new@example.com";
        const string newName = "New Name";
        const string newPasswordHash = "new_hash";
        DateTime newLastLogin = DateTime.UtcNow;

        user.Email = newEmail;
        user.Name = newName;
        user.PasswordHash = newPasswordHash;
        user.LastLogin = newLastLogin;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(user.Email, Is.EqualTo(newEmail));
            Assert.That(user.Name, Is.EqualTo(newName));
            Assert.That(user.PasswordHash, Is.EqualTo(newPasswordHash));
            Assert.That(user.Roles, Is.EqualTo(UserInitRoles));
            Assert.That(user.LastLogin, Is.EqualTo(newLastLogin));
        }
    }
    
    [Test]
    public void SetProperty_WhenNameChanged_RaisesPropertyChangedEvent()
    {
        // Arrange
        var user = new User { Name = "Initial Name", Email = "initial@test.com", PasswordHash = "initial_hash", Roles = new List<Role>() };
        string? raisedPropertyName = null;
        user.PropertyChanged += (sender, args) => { raisedPropertyName = args.PropertyName; };

        // Act
        user.Name = "New Name";

        // Assert
        Assert.That(raisedPropertyName, Is.EqualTo(nameof(User.Name)));
    }

    [Test]
    public void SetProperty_WhenNameUnchanged_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var user = new User { Name = "Initial Name", Email = "initial@test.com", PasswordHash = "initial_hash", Roles = new List<Role>() };
        string? raisedPropertyName = null;
        user.PropertyChanged += (sender, args) => { raisedPropertyName = args.PropertyName; };

        // Act
        user.Name = "Initial Name";

        // Assert
        Assert.That(raisedPropertyName, Is.Null);
    }
    
    [Test]
    public void SetProperty_WhenLastLoginChanged_RaisesPropertyChangedEvent()
    {
        // Arrange
        var user = new User { Name = "Initial Name", Email = "initial@test.com", PasswordHash = "initial_hash", Roles = new List<Role>() };
        string? raisedPropertyName = null;
        user.PropertyChanged += (sender, args) => { raisedPropertyName = args.PropertyName; };

        // Act
        user.LastLogin = DateTime.UtcNow;

        // Assert
        Assert.That(raisedPropertyName, Is.EqualTo(nameof(User.LastLogin)));
    }

    [Test]
    public void SetProperty_WhenLastLoginUnchanged_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var lastLogin = DateTime.UtcNow;
        var user = new User { Name = "Initial Name", Email = "initial@test.com", PasswordHash = "initial_hash", Roles = new List<Role>(), LastLogin = lastLogin };
        string? raisedPropertyName = null;
        user.PropertyChanged += (sender, args) => { raisedPropertyName = args.PropertyName; };

        // Act
        user.LastLogin = lastLogin;

        // Assert
        Assert.That(raisedPropertyName, Is.Null);
    }
}
