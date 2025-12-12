using System.ComponentModel.DataAnnotations;
using Jahoot.Core.Attributes;

namespace Jahoot.Core.Models.Requests;

public class CreateStudentRequestModel
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    [StrongPassword]
    public required string Password { get; init; }

    [Required]
    [StringLength(70, MinimumLength = 2)]
    public required string Name { get; init; }
}
