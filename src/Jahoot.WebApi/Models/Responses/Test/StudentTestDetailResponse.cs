namespace Jahoot.WebApi.Models.Responses.Test;

public class StudentTestDetailResponse
{
    public int TestId { get; init; }
    public required string Name { get; init; }
    public required string SubjectName { get; init; }
    public int NumberOfQuestions { get; init; }
    public required IEnumerable<StudentQuestionResponse> Questions { get; init; }
}

public class StudentQuestionResponse
{
    public int QuestionId { get; init; }
    public required string Text { get; init; }
    public required IEnumerable<StudentQuestionOptionResponse> Options { get; init; }
}

public class StudentQuestionOptionResponse
{
    public int QuestionOptionId { get; init; }
    public required string OptionText { get; init; }
}
