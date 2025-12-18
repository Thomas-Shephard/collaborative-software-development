namespace Jahoot.Core.Models;

public class Test
{
    public int TestId { get; init; }
    public int SubjectId { get; set; }
    public required string Name { get; set; }
    public int NumberOfQuestions { get; set; }
    public required IReadOnlyList<Question> Questions { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
