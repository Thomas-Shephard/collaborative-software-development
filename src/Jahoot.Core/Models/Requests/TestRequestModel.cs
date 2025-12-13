using System.ComponentModel.DataAnnotations;

namespace Jahoot.Core.Models.Requests;

public class TestRequestModel
{
    [Required]
    public int SubjectId { get; init; }
    [Required]
    public required string Name { get; init; }
    [Required]
    [MinLength(1, ErrorMessage = "A test must have at least 1 question.")]
    public required List<QuestionRequestModel> Questions { get; init; }
}
