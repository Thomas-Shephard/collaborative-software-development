using Jahoot.Core.Models;

namespace Jahoot.Core.Tests.Models;

public class QuestionTests
{
    [Test]
    public void QuestionProperties_CanBeAccessed()
    {
        const int questionId = 1;
        const string text = "What is 2+2?";
        DateTime now = DateTime.UtcNow;
        List<QuestionOption> options =
        [
            new() { OptionText = "4", IsCorrect = true },
            new() { OptionText = "5", IsCorrect = false }
        ];

        Question question = new()
        {
            QuestionId = questionId,
            Text = text,
            Options = options.AsReadOnly(),
            CreatedAt = now,
            UpdatedAt = now
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(question.QuestionId, Is.EqualTo(questionId));
            Assert.That(question.Text, Is.EqualTo(text));
            Assert.That(question.Options, Is.EquivalentTo(options));
            Assert.That(question.CreatedAt, Is.EqualTo(now));
            Assert.That(question.UpdatedAt, Is.EqualTo(now));
        }
    }
}
