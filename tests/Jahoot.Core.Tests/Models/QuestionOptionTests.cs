using Jahoot.Core.Models;

namespace Jahoot.Core.Tests.Models;

public class QuestionOptionTests
{
    [Test]
    public void QuestionOptionProperties_CanBeAccessed()
    {
        const int questionOptionId = 1;
        const int questionId = 10;
        const string optionText = "Option A";
        const bool isCorrect = true;

        QuestionOption option = new()
        {
            QuestionOptionId = questionOptionId,
            QuestionId = questionId,
            OptionText = optionText,
            IsCorrect = isCorrect
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(option.QuestionOptionId, Is.EqualTo(questionOptionId));
            Assert.That(option.QuestionId, Is.EqualTo(questionId));
            Assert.That(option.OptionText, Is.EqualTo(optionText));
            Assert.That(option.IsCorrect, Is.EqualTo(isCorrect));
        }
    }
}
