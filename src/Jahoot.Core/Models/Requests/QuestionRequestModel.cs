using System.ComponentModel.DataAnnotations;

namespace Jahoot.Core.Models.Requests;

public class QuestionRequestModel : IValidatableObject
{
    [Required]
    public required string Text { get; init; }

    [Required]
    [MinLength(2, ErrorMessage = "A question must have at least 2 options.")]
    public required IReadOnlyList<QuestionOptionRequestModel> Options { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Options.Any(option => option.IsCorrect))
        {
            yield return new ValidationResult("At least one option must be marked as correct.", [nameof(Options)]);
        }
    }
}
