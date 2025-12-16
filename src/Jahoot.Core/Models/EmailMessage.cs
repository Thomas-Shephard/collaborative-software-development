namespace Jahoot.Core.Models;

public class EmailMessage
{
    public required string To { get; init; }
    public required string Subject { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
}
