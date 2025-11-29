using System.ComponentModel.DataAnnotations;

namespace Jahoot.Core.Models;

public class ResetPasswordRequest
{
    [Required]
    public required string Token { get; init; }

    [Required]
    [MinLength(8)]
    public required string NewPassword { get; init; }
}
