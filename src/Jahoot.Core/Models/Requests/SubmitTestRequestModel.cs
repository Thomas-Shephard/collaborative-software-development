namespace Jahoot.Core.Models.Requests;

public class SubmitTestRequestModel
{
    public required List<AnswerRequestModel> Answers { get; init; }
}

public class AnswerRequestModel
{
    public int QuestionId { get; init; }
    public int SelectedOptionId { get; init; }
}
