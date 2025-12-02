using Jahoot.Core.Models;

namespace Jahoot.Core.Tests.Models;

[TestFixture]
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
}
