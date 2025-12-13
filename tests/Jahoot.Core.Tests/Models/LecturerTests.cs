using Jahoot.Core.Models;

namespace Jahoot.Core.Tests.Models;

public class LecturerTests
{
    [Test]
    public void LecturerProperties_CanBeAccessed()
    {
        const int lecturerId = 1;
        const bool isAdmin = true;
        const int userId = 101;
        const string userEmail = "lecturer@example.com";
        const string userName = "Dr. Lecturer";
        const string userPasswordHash = "hashed_password";
        List<Role> userRoles = [Role.Lecturer, Role.Admin];

        Lecturer lecturer = new()
        {
            LecturerId = lecturerId,
            IsAdmin = isAdmin,
            UserId = userId,
            Email = userEmail,
            Name = userName,
            PasswordHash = userPasswordHash,
            Roles = userRoles
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(lecturer.LecturerId, Is.EqualTo(lecturerId));
            Assert.That(lecturer.IsAdmin, Is.EqualTo(isAdmin));
            Assert.That(lecturer.UserId, Is.EqualTo(userId));
            Assert.That(lecturer.Email, Is.EqualTo(userEmail));
            Assert.That(lecturer.Name, Is.EqualTo(userName));
            Assert.That(lecturer.PasswordHash, Is.EqualTo(userPasswordHash));
            Assert.That(lecturer.Roles, Is.EquivalentTo(userRoles));
        }
    }
}
