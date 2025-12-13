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

    public bool IsAdmin { get; init; }
}
