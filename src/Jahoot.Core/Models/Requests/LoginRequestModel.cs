using System.ComponentModel.DataAnnotations;

namespace Jahoot.Core.Models.Requests;

public class LoginRequestModel
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    [MinLength(8)]
    public required string Password { get; init; }
}
