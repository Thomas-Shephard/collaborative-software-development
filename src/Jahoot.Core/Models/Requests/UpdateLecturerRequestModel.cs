using System.ComponentModel.DataAnnotations;

namespace Jahoot.Core.Models.Requests;

public class UpdateLecturerRequestModel
{
    [Required]
    [StringLength(70, MinimumLength = 2)]
    public required string Name { get; init; }

    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required bool IsAdmin { get; init; }

    [Required]
    public required bool IsDisabled { get; init; }
}
