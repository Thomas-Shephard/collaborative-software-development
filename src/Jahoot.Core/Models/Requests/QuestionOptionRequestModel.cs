using System.ComponentModel.DataAnnotations;

namespace Jahoot.Core.Models.Requests;

public class QuestionOptionRequestModel
{
    [Required]
    public required string OptionText { get; init; }
    public bool IsCorrect { get; init; }
}
