using Jahoot.Core.Models;

namespace Jahoot.Core.Tests.Models;

[TestFixture]
public class StudentTests
{
    [Test]
    public void StudentProperties_CanBeAccessed()
    {
        const int studentId = 1;
        const bool isApproved = true;
        const bool isDisabled = false;
        const int userId = 101;
        const string userEmail = "test@example.com";
        const string userName = "John Doe";
        const string userPasswordHash = "hashed_password";
        List<Role> userRoles = [ Role.Student ];
        List<Subject> userSubjects = [ new() { Name = "Subject",  SubjectId = 1 } ];

        Student student = new()
        {
            StudentId = studentId,
            IsApproved = isApproved,
            IsDisabled = isDisabled,
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
            Assert.That(student.IsApproved, Is.EqualTo(isApproved));
            Assert.That(student.IsDisabled, Is.EqualTo(isDisabled));
            Assert.That(student.UserId, Is.EqualTo(userId));
            Assert.That(student.Email, Is.EqualTo(userEmail));
            Assert.That(student.Name, Is.EqualTo(userName));
            Assert.That(student.PasswordHash, Is.EqualTo(userPasswordHash));
            Assert.That(student.Roles, Is.EquivalentTo(userRoles));
            Assert.That(student.Subjects, Is.EquivalentTo(userSubjects));
        }
    }

    [Test]
    public void IsApproved_Set_RaisesPropertyChanged()
    {
        Student student = new() { Subjects = [], Email = "a@a.com", Name = "a", Roles = [] };
        string? raisedPropertyName = null;
        student.PropertyChanged += (_, args) => raisedPropertyName = args.PropertyName;

        student.IsApproved = true;

        Assert.That(raisedPropertyName, Is.EqualTo(nameof(Student.IsApproved)));
    }

    [TestCase("John Doe", "JD")]
    [TestCase("john doe", "JD")]
    [TestCase("Single", "S")]
    [TestCase("Multiple Name Parts Here", "MH")]
    [TestCase("  Trim Test  ", "TT")]
    [TestCase("", "")]
    [TestCase(" ", "")]
    [TestCase(null, "")]
    public void Initials_ReturnsExpectedValue(string? name, string expected)
    {
        Student student = new() { Name = name!, Subjects = [], Email = "", Roles = [] };
        Assert.That(student.Initials, Is.EqualTo(expected));
    }

    [Test]
    public void Equals_SameUserId_ReturnsTrue()
    {
        Student student1 = new() { UserId = 1, Subjects = [], Email = "", Name = "", Roles = [] };
        Student student2 = new() { UserId = 1, Subjects = [], Email = "", Name = "", Roles = [] };

        Assert.That(student1, Is.EqualTo(student2));
    }

    [Test]
    public void Equals_DifferentUserId_ReturnsFalse()
    {
        Student student1 = new() { UserId = 1, Subjects = [], Email = "", Name = "", Roles = [] };
        Student student2 = new() { UserId = 2, Subjects = [], Email = "", Name = "", Roles = [] };

        Assert.That(student1, Is.Not.EqualTo(student2));
    }

    [Test]
    public void Equals_NullOrDifferentType_ReturnsFalse()
    {
        Student student = new() { UserId = 1, Subjects = [], Email = "", Name = "", Roles = [] };
        const string notAStudent = "Not a Student";

        // ReSharper disable once SuspiciousTypeConversion.Global
        bool isEqual = student.Equals(notAStudent);

        Assert.That(isEqual, Is.False);
    }

    [Test]
    public void GetHashCode_SameUserId_ReturnsSameHashCode()
    {
        Student student1 = new() { UserId = 1, Subjects = [], Email = "", Name = "", Roles = [] };
        Student student2 = new() { UserId = 1, Subjects = [], Email = "", Name = "", Roles = [] };

        Assert.That(student1.GetHashCode(), Is.EqualTo(student2.GetHashCode()));
    }
}
