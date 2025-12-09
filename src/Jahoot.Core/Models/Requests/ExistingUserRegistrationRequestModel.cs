using System.ComponentModel.DataAnnotations;

namespace Jahoot.Core.Models.Requests;

public class ExistingUserRegistrationRequestModel
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}
