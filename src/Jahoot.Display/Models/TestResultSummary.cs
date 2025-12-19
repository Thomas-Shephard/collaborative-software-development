namespace Jahoot.Display.Models;

public class TestResultSummary
{
    public required string TestName { get; init; }
    public required string SubjectName { get; init; }
    public int TotalQuestions { get; init; }
    public int CorrectAnswers { get; init; }
    public int IncorrectAnswers { get; init; }
    public double ScorePercentage { get; init; }
    public string Grade { get; init; } = string.Empty;
    public DateTime CompletedAt { get; init; }
    public TimeSpan TimeTaken { get; init; }
}

public class QuestionReviewItem
{
    public int QuestionNumber { get; init; }
    public required string QuestionText { get; init; }
    public required List<AnswerOptionReview> Options { get; init; }
    public bool IsCorrect { get; init; }
}

public class AnswerOptionReview
{
    public required string OptionText { get; init; }
    public bool IsCorrectAnswer { get; init; }
    public bool WasSelected { get; init; }
}
