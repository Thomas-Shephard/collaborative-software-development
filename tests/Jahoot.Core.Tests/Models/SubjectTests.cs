using Jahoot.Core.Models;

namespace Jahoot.Core.Tests.Models;

public class SubjectTests
{
    [Test]
    public void SubjectProperties_CanBeAccessed()
    {
        const int subjectId = 1;
        const string name = "Mathematics";
        const bool isActive = true;
        DateTime now = DateTime.UtcNow;

        Subject subject = new()
        {
            SubjectId = subjectId,
            Name = name,
            IsActive = isActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(subject.SubjectId, Is.EqualTo(subjectId));
            Assert.That(subject.Name, Is.EqualTo(name));
            Assert.That(subject.IsActive, Is.EqualTo(isActive));
            Assert.That(subject.CreatedAt, Is.EqualTo(now));
            Assert.That(subject.UpdatedAt, Is.EqualTo(now));
        }
    }

    [Test]
    public void SubjectProperties_CanBeSet()
    {
        Subject subject = new()
        {
            Name = "Initial Name",
            IsActive = false
        };

        const string newName = "New Name";
        const bool newIsActive = true;

        subject.Name = newName;
        subject.IsActive = newIsActive;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(subject.Name, Is.EqualTo(newName));
            Assert.That(subject.IsActive, Is.EqualTo(newIsActive));
        }
    }
}
