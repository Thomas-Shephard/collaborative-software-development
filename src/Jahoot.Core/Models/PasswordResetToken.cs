namespace Jahoot.Core.Models;

public class PasswordResetToken
{
    public int TokenId { get; init; }
    public int UserId { get; init; }
    public required string Token { get; init; }
    public DateTime Expiration { get; init; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; init; }
}
