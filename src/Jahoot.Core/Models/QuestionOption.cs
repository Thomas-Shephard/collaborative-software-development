namespace Jahoot.Core.Models;

public class QuestionOption
{
    public int QuestionOptionId { get; init; }
    public int QuestionId { get; init; }
    public required string OptionText { get; init; }
    public bool IsCorrect { get; init; }
}
