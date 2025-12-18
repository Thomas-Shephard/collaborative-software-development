using System.ComponentModel.DataAnnotations;

namespace Jahoot.Core.Models.Requests;

public class SubmitTestRequestModel
{
    [Required]
    public required List<AnswerRequestModel> Answers { get; init; }
}

public class AnswerRequestModel
{
    [Required]
    public int QuestionId { get; init; }

    [Required]
    public int SelectedOptionId { get; init; }
}
