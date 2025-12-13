namespace Jahoot.Core.Models;

public class Question
{
    public int QuestionId { get; init; }
    public required string Text { get; init; }
    public required IReadOnlyList<QuestionOption> Options { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
