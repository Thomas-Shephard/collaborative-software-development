using Jahoot.Core.Models;

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
}
