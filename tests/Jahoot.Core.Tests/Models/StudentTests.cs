using Jahoot.Core.Models;
using System.ComponentModel;

namespace Jahoot.Core.Tests.Models;

[TestFixture]
public class StudentTests
{
    [Test]
    public void StudentProperties_CanBeAccessed()
    {
        const int studentId = 1;
        const StudentAccountStatus studentAccountStatus = StudentAccountStatus.Active;
        const int userId = 101;
        const string userEmail = "test@example.com";
        const string userName = "John Doe";
        const string userPasswordHash = "hashed_password";
        List<Role> userRoles = [ Role.Student ];
        List<Subject> userSubjects = [ new() { Name = "Subject",  SubjectId = 1 } ];

        Student student = new()
        {
            StudentId = studentId,
            AccountStatus = studentAccountStatus,
            UserId = userId,
            Email = userEmail,
            Name = userName,
            PasswordHash = userPasswordHash,
            Roles = userRoles,
            Subjects = userSubjects
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(student.StudentId, Is.EqualTo(studentId));
            Assert.That(student.AccountStatus, Is.EqualTo(studentAccountStatus));
            Assert.That(student.UserId, Is.EqualTo(userId));
            Assert.That(student.Email, Is.EqualTo(userEmail));
            Assert.That(student.Name, Is.EqualTo(userName));
            Assert.That(student.PasswordHash, Is.EqualTo(userPasswordHash));
            Assert.That(student.Roles, Is.EquivalentTo(userRoles));
            Assert.That(student.Subjects, Is.EquivalentTo(userSubjects));
        }
    }

    [TestCase("John Doe", "JD")]
    [TestCase("jane doe", "JD")]
    [TestCase(" single", "S")]
    [TestCase("John", "J")]
    [TestCase("John Fitzgerald Kennedy", "JK")]
    [TestCase("  leading space", "LS")]
    public void Initials_ReturnsCorrectInitials_ForGivenName(string name, string expectedInitial)
    {
        // Arrange
        var student = new Student { Name = name, AccountStatus = StudentAccountStatus.Active, Email = "test@test.com", PasswordHash = "testhash", Roles = new List<Role>() };

        // Act
        var initials = student.Initials;

        // Assert
        Assert.That(initials, Is.EqualTo(expectedInitial));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Initials_ReturnsEmptyString_WhenNameIsNullOrEmptyOrWhitespace(string name)
    {
        // Arrange
        var student = new Student { Name = name, AccountStatus = StudentAccountStatus.Active, Email = "test@test.com", PasswordHash = "testhash", Roles = new List<Role>() };

        // Act
        var initials = student.Initials;

        // Assert
        Assert.That(initials, Is.Empty);
    }

    [Test]
    public void Equals_ReturnsTrue_WhenComparingTwoStudentsWithTheSameId()
    {
        // Arrange
        var student1 = new Student { UserId = 1, Name = "Student 1", AccountStatus = StudentAccountStatus.Active, Email = "test@test.com", PasswordHash = "testhash", Roles = new List<Role>() };
        var student2 = new Student { UserId = 1, Name = "Student 2", AccountStatus = StudentAccountStatus.Disabled, Email = "test@test.com", PasswordHash = "testhash", Roles = new List<Role>() };

        // Act & Assert
        Assert.That(student1.Equals(student2), Is.True);
    }

    [Test]
    public void Equals_ReturnsFalse_WhenComparingTwoStudentsWithDifferentIds()
    {
        // Arrange
        var student1 = new Student { UserId = 1, Name = "Student", AccountStatus = StudentAccountStatus.Active, Email = "test@test.com", PasswordHash = "testhash", Roles = new List<Role>() };
        var student2 = new Student { UserId = 2, Name = "Student", AccountStatus = StudentAccountStatus.Active, Email = "test@test.com", PasswordHash = "testhash", Roles = new List<Role>() };

        // Act & Assert
        Assert.That(student1.Equals(student2), Is.False);
    }

    [Test]
    public void Equals_ReturnsFalse_WhenComparingWithNull()
    {
        // Arrange
        var student = new Student { UserId = 1, Name = "Student", AccountStatus = StudentAccountStatus.Active, Email = "test@test.com", PasswordHash = "testhash", Roles = new List<Role>() };

        // Act & Assert
        Assert.That(student.Equals(null), Is.False);
    }

    [Test]
    public void Equals_ReturnsFalse_WhenComparingWithDifferentType()
    {
        // Arrange
        var student = new Student { UserId = 1, Name = "Student", AccountStatus = StudentAccountStatus.Active, Email = "test@test.com", PasswordHash = "testhash", Roles = new List<Role>() };
        var other = new object();

        // Act & Assert
        Assert.That(student.Equals(other), Is.False);
    }

    [Test]
    public void GetHashCode_ReturnsSameValue_ForTwoStudentsWithTheSameId()
    {
        // Arrange
        var student1 = new Student { UserId = 1, Name = "Student 1", AccountStatus = StudentAccountStatus.Active, Email = "test@test.com", PasswordHash = "testhash", Roles = new List<Role>() };
        var student2 = new Student { UserId = 1, Name = "Student 2", AccountStatus = StudentAccountStatus.Disabled, Email = "test@test.com", PasswordHash = "testhash", Roles = new List<Role>() };

        // Act & Assert
        Assert.That(student1.GetHashCode(), Is.EqualTo(student2.GetHashCode()));
    }

    [Test]
    public void GetHashCode_ReturnsDifferentValue_ForTwoStudentsWithDifferentIds()
    {
        // Arrange
        var student1 = new Student { UserId = 1, Name = "Student", AccountStatus = StudentAccountStatus.Active, Email = "test@test.com", PasswordHash = "testhash", Roles = new List<Role>() };
        var student2 = new Student { UserId = 2, Name = "Student", AccountStatus = StudentAccountStatus.Active, Email = "test@test.com", PasswordHash = "testhash", Roles = new List<Role>() };

        // Act & Assert
        Assert.That(student1.GetHashCode(), Is.Not.EqualTo(student2.GetHashCode()));
    }

    [Test]
    public void SetProperty_WhenAccountStatusChanged_RaisesPropertyChangedEvent()
    {
        // Arrange
        var student = new Student { Name = "Initial Name", Email = "initial@test.com", PasswordHash = "initial_hash", Roles = new List<Role>(), AccountStatus = StudentAccountStatus.Active };
        string? raisedPropertyName = null;
        student.PropertyChanged += (sender, args) => { raisedPropertyName = args.PropertyName; };

        // Act
        student.AccountStatus = StudentAccountStatus.Disabled;

        // Assert
        Assert.That(raisedPropertyName, Is.EqualTo(nameof(Student.AccountStatus)));
    }

    [Test]
    public void SetProperty_WhenAccountStatusUnchanged_DoesNotRaisePropertyChangedEvent()
    {
        // Arrange
        var student = new Student { Name = "Initial Name", Email = "initial@test.com", PasswordHash = "initial_hash", Roles = new List<Role>(), AccountStatus = StudentAccountStatus.Active };
        string? raisedPropertyName = null;
        student.PropertyChanged += (sender, args) => { raisedPropertyName = args.PropertyName; };

        // Act
        student.AccountStatus = StudentAccountStatus.Active;

        // Assert
        Assert.That(raisedPropertyName, Is.Null);
    }
}