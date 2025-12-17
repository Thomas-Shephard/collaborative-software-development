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

    private async Task<int> InsertUser(User user)
    {
        const string insertQuery = "INSERT INTO User (email, name, password_hash, created_at, updated_at) VALUES (@Email, @Name, @PasswordHash, @CreatedAt, @UpdatedAt); SELECT LAST_INSERT_ID();";
        return await Connection.QuerySingleAsync<int>(insertQuery, user);
    }

    private async Task InsertLecturer(int userId, bool isAdmin)
    {
        await Connection.ExecuteAsync("INSERT INTO Lecturer (user_id, is_admin) VALUES (@UserId, @IsAdmin)", new { UserId = userId, IsAdmin = isAdmin });
    }

    private async Task InsertStudent(int userId, bool isApproved = false)
    {
        await Connection.ExecuteAsync("INSERT INTO Student (user_id, is_approved) VALUES (@UserId, @IsApproved)", new { UserId = userId, IsApproved = isApproved });
    }

    [Test]
    public async Task GetUserByEmailAsync_UserExistsWithoutRoles_ReturnsUserWithEmptyRoles()
    {
        User user = new()
        {
            Email = "test@example.com",
            Name = "Test User",
            PasswordHash = "password_hash",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastLogin = null,
            Roles = [],
            IsDisabled = false
        };
        await InsertUser(user);

        User? result = await _userRepository.GetUserByEmailAsync("test@example.com");

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Email, Is.EqualTo(user.Email));
            Assert.That(result.Roles, Is.Empty);
        }
    }

    [Test]
    public async Task GetUserByEmailAsync_UserExistsAsStudent_ReturnsUserWithStudentRole()
    {
        User user = new()
        {
            Email = "student@example.com",
            Name = "Student User",
            PasswordHash = "password_hash",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastLogin = null,
            Roles = [],
            IsDisabled = false
        };
        int userId = await InsertUser(user);
        await InsertStudent(userId, true);

        User? result = await _userRepository.GetUserByEmailAsync("student@example.com");

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Email, Is.EqualTo(user.Email));
            Assert.That(result.Roles, Is.EquivalentTo([Role.Student]));
        }
    }

    [Test]
    public async Task GetUserByEmailAsync_UserExistsAsLecturer_ReturnsUserWithLecturerRole()
    {
        User user = new()
        {
            Email = "lecturer@example.com",
            Name = "Lecturer User",
            PasswordHash = "password_hash",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastLogin = null,
            Roles = [],
            IsDisabled = false
        };
        int userId = await InsertUser(user);
        await InsertLecturer(userId, false); // Not an admin lecturer

        User? result = await _userRepository.GetUserByEmailAsync("lecturer@example.com");

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Email, Is.EqualTo(user.Email));
            Assert.That(result.Roles, Is.EquivalentTo([Role.Lecturer]));
        }
    }

    [Test]
    public async Task GetUserByEmailAsync_UserExistsAsAdminLecturer_ReturnsUserWithAdminAndLecturerRoles()
    {
        User user = new()
        {
            Email = "admin@example.com",
            Name = "Admin User",
            PasswordHash = "password_hash",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastLogin = null,
            Roles = [],
            IsDisabled = false
        };
        int userId = await InsertUser(user);
        await InsertLecturer(userId, true); // Admin lecturer

        User? result = await _userRepository.GetUserByEmailAsync("admin@example.com");

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Email, Is.EqualTo(user.Email));
            Assert.That(result.Roles, Is.EquivalentTo([Role.Lecturer, Role.Admin]));
        }
    }

    [Test]
    public async Task GetUserByEmailAsync_UserExistsAsStudentAndLecturer_ReturnsUserWithBothRoles()
    {
        User user = new()
        {
            Email = "studentlecturer@example.com",
            Name = "Student Lecturer User",
            PasswordHash = "password_hash",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastLogin = null,
            Roles = [],
            IsDisabled = false
        };
        int userId = await InsertUser(user);
        await InsertStudent(userId, true);
        await InsertLecturer(userId, false); // Not an admin lecturer

        User? result = await _userRepository.GetUserByEmailAsync("studentlecturer@example.com");

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Email, Is.EqualTo(user.Email));
            Assert.That(result.Roles, Is.EquivalentTo([Role.Student, Role.Lecturer]));
        }
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
            UpdatedAt = DateTime.UtcNow,
            Roles = [],
            IsDisabled = false
        };

        int userId = await InsertUser(originalUser);

        User updatedUser = new()
        {
            UserId = userId,
            Email = "updated-user@example.com",
            Name = "Updated Name",
            PasswordHash = "updated_hash",
            CreatedAt = originalUser.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            LastLogin = DateTime.UtcNow,
            Roles = [],
            IsDisabled = true
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
            Assert.That(result.IsDisabled, Is.EqualTo(updatedUser.IsDisabled));
        }
    }

    [Test]
    public async Task GetUserByEmailAsync_UserIsDisabled_ReturnsUserWithoutRoles()
    {
        User user = new()
        {
            Email = "disabled@example.com",
            Name = "Disabled User",
            PasswordHash = "password_hash",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastLogin = null,
            Roles = [],
            IsDisabled = true
        };
        int userId = await InsertUser(user);
        await Connection.ExecuteAsync("UPDATE User SET is_disabled = TRUE WHERE user_id = @UserId", new { UserId = userId });
        await InsertStudent(userId, true); // Approved student, but user is disabled

        User? result = await _userRepository.GetUserByEmailAsync("disabled@example.com");

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Email, Is.EqualTo(user.Email));
            Assert.That(result.Roles, Is.Empty); // No roles because user is disabled
            Assert.That(result.IsDisabled, Is.True);
        }
    }

    [Test]
    public async Task GetRolesByUserIdAsync_UserIsStudent_ReturnsStudentRole()
    {
        User user = new()
        {
            Email = "student_role@example.com",
            Name = "Student Role",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Roles = [],
            IsDisabled = false
        };
        int userId = await InsertUser(user);
        await InsertStudent(userId, true);

        List<Role> roles = await _userRepository.GetRolesByUserIdAsync(userId);

        Assert.That(roles, Is.EquivalentTo([Role.Student]));
    }

    [Test]
    public async Task GetRolesByUserIdAsync_UserIsLecturer_ReturnsLecturerRole()
    {
        User user = new()
        {
            Email = "lecturer_role@example.com",
            Name = "Lecturer Role",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Roles = [],
            IsDisabled = false
        };
        int userId = await InsertUser(user);
        await InsertLecturer(userId, false);

        List<Role> roles = await _userRepository.GetRolesByUserIdAsync(userId);

        Assert.That(roles, Is.EquivalentTo([Role.Lecturer]));
    }

    [Test]
    public async Task GetRolesByUserIdAsync_UserIsAdmin_ReturnsAdminAndLecturerRoles()
    {
        User user = new()
        {
            Email = "admin_role@example.com",
            Name = "Admin Role",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Roles = [],
            IsDisabled = false
        };
        int userId = await InsertUser(user);
        await InsertLecturer(userId, true);

        List<Role> roles = await _userRepository.GetRolesByUserIdAsync(userId);

        Assert.That(roles, Is.EquivalentTo([Role.Lecturer, Role.Admin]));
    }

    [Test]
    public async Task GetRolesByUserIdAsync_StudentNotApproved_ReturnsEmpty()
    {
        User user = new()
        {
            Email = "unapproved@example.com",
            Name = "Unapproved Student",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Roles = [],
            IsDisabled = false
        };
        int userId = await InsertUser(user);
        await InsertStudent(userId, false); // Not approved

        List<Role> roles = await _userRepository.GetRolesByUserIdAsync(userId);

        Assert.That(roles, Is.Empty);
    }

    [Test]
    public async Task GetRolesByUserIdAsync_UserIsDisabled_ReturnsEmpty()
    {
        User user = new()
        {
            Email = "disabled_role@example.com",
            Name = "Disabled Role",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Roles = [],
            IsDisabled = true
        };
        int userId = await InsertUser(user);
        await Connection.ExecuteAsync("UPDATE User SET is_disabled = TRUE WHERE user_id = @UserId", new { UserId = userId });
        await InsertStudent(userId, true); // Approved student, but disabled user

        List<Role> roles = await _userRepository.GetRolesByUserIdAsync(userId);

        Assert.That(roles, Is.Empty);
    }
}
