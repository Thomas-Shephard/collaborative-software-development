using System.ComponentModel.DataAnnotations;
using Jahoot.Core.Attributes;

namespace Jahoot.Core.Models;

public class ResetPasswordRequest
{
    [Required]
    public required string Token { get; init; }

    [Required]
    [StrongPassword]
    public required string NewPassword { get; init; }
}
