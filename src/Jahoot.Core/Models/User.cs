using System.Text.Json.Serialization;

namespace Jahoot.Core.Models;

public class User
{
    public int UserId { get; init; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    [JsonIgnore]
    public string? PasswordHash { get; set; }
    public required IReadOnlyList<Role> Roles { get; init; }
    public bool IsDisabled { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
