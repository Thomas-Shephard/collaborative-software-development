using System.ComponentModel.DataAnnotations;
using Jahoot.Core.Attributes;

namespace Jahoot.Core.Models.Requests;

public class UpdateStudentRequestModel
{
    [Required]
    [StringLength(70, MinimumLength = 2)]
    public required string Name { get; init; }

    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required bool IsApproved { get; init; }

    [Required]
    public required bool IsDisabled { get; init; }

    [Required]
    [EnsureUniqueElements]
    public required IReadOnlyList<int> SubjectIds { get; init; }
}
