using System.ComponentModel.DataAnnotations;

namespace Jahoot.Core.Models.Requests;

public class TestRequestModel : IValidatableObject
{
    [Required]
    public int SubjectId { get; init; }
    [Required]
    public required string Name { get; init; }
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "A test must have at least 1 question.")]
    public int NumberOfQuestions { get; init; }
    [Required]
    [MinLength(1, ErrorMessage = "A test must have at least 1 question.")]
    public required List<QuestionRequestModel> Questions { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NumberOfQuestions > Questions.Count)
        {
            yield return new ValidationResult("The number of questions cannot exceed the total number of available questions.", [nameof(NumberOfQuestions)]);
        }
    }
}
