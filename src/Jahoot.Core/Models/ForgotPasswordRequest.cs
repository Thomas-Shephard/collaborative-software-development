using System.ComponentModel.DataAnnotations;

namespace Jahoot.Core.Models;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }
}
