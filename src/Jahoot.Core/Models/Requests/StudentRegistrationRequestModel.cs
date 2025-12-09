using System.ComponentModel.DataAnnotations;
using Jahoot.Core.Attributes;

namespace Jahoot.Core.Models.Requests;

public class StudentRegistrationRequestModel
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    [StrongPassword]
    public required string Password { get; init; }

    [Required]
    [MaxLength(70)]
    public required string Name { get; init; }
}
