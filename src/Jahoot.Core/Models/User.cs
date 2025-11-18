namespace Jahoot.Core.Models;

public class User
{
    public int Id { get; init; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
