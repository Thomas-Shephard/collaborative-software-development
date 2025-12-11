using System.ComponentModel.DataAnnotations;

namespace Jahoot.Core.Models.Requests;

public class UpdateStudentRequestModel
{
    [Required]
    [MaxLength(70)]
    public required string Name { get; init; }

    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required StudentAccountStatus AccountStatus { get; init; }
}
