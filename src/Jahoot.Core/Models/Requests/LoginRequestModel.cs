using System.ComponentModel.DataAnnotations;
using Jahoot.Core.Attributes;

namespace Jahoot.Core.Models.Requests;

public class LoginRequestModel
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    [StrongPassword]
    public required string Password { get; init; }
}
