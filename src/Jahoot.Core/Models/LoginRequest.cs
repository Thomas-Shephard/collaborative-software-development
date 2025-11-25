using System.ComponentModel.DataAnnotations;

namespace Jahoot.Core.Models;

/// <summary>
/// This is what we need to log a user in.
/// It's just their email and password.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// The user's email address.
    /// </summary>
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    /// <summary>
    /// The user's password.
    /// </summary>
    [Required]
    [MinLength(8)]
    public required string Password { get; init; }
}
