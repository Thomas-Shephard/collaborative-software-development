namespace Jahoot.Core.Models;

/// <summary>
/// This is a user of our app.
/// It has all their details.
/// </summary>
public class User
{
    /// <summary>
    /// The user's special ID number.
    /// </summary>
    public int UserId { get; init; }
    /// <summary>
    /// The user's email address.
    /// </summary>
    public required string Email { get; set; }
    /// <summary>
    /// The user's name.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// The user's password, but all scrambled up so it's safe.
    /// </summary>
    public required string PasswordHash { get; set; }
    /// <summary>
    /// When the user last logged in.
    /// </summary>
    public DateTime? LastLogin { get; set; }
    /// <summary>
    /// When the user's account was made.
    /// </summary>
    public DateTime CreatedAt { get; init; }
    /// <summary>
    /// When the user's account was last changed.
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}
