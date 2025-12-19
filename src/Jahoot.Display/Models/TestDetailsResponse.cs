namespace Jahoot.Display.Models;

/// <summary>
/// Represents test details response from the API including questions.
/// </summary>
public class TestDetailsResponse
{
    public int TestId { get; init; }
    public required string Name { get; init; }
    public required string SubjectName { get; init; }
    public int NumberOfQuestions { get; init; }
    public required IEnumerable<TestQuestionResponse> Questions { get; init; }
}

public class TestQuestionResponse
{
    public int QuestionId { get; init; }
    public required string Text { get; init; }
    public required IEnumerable<TestQuestionOptionResponse> Options { get; init; }
}

public class TestQuestionOptionResponse
{
    public int QuestionOptionId { get; init; }
    public required string OptionText { get; init; }
}
