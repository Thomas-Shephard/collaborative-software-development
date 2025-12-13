using Jahoot.Core.Models;
using TestModel = Jahoot.Core.Models.Test;

namespace Jahoot.Core.Tests.Models;

public class TestTests
{
    [Test]
    public void TestProperties_CanBeAccessed()
    {
        const int testId = 1;
        const int subjectId = 5;
        const string name = "Maths Quiz";
        DateTime now = DateTime.UtcNow;
        List<Question> questions =
        [
            new() { Text = "Q1", Options = [new QuestionOption { OptionText = "A", IsCorrect = true }] }
        ];

        TestModel test = new()
        {
            TestId = testId,
            SubjectId = subjectId,
            Name = name,
            Questions = questions.AsReadOnly(),
            CreatedAt = now,
            UpdatedAt = now
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.TestId, Is.EqualTo(testId));
            Assert.That(test.SubjectId, Is.EqualTo(subjectId));
            Assert.That(test.Name, Is.EqualTo(name));
            Assert.That(test.Questions, Is.EquivalentTo(questions));
            Assert.That(test.CreatedAt, Is.EqualTo(now));
            Assert.That(test.UpdatedAt, Is.EqualTo(now));
        }
    }

    [Test]
    public void TestProperties_CanBeSet()
    {
        TestModel test = new()
        {
            SubjectId = 1,
            Name = "Old Name",
            Questions = []
        };

        const int newSubjectId = 2;
        const string newName = "New Name";
        List<Question> newQuestions = [new() { Text = "New Q", Options = [] }];

        test.SubjectId = newSubjectId;
        test.Name = newName;
        test.Questions = newQuestions.AsReadOnly();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(test.SubjectId, Is.EqualTo(newSubjectId));
            Assert.That(test.Name, Is.EqualTo(newName));
            Assert.That(test.Questions, Is.EquivalentTo(newQuestions));
        }
    }
}
