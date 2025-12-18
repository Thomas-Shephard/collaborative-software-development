namespace Jahoot.Display.Models;

/// <summary>
/// Represents an upcoming test response from the API.
/// </summary>
public class UpcomingTestResponse
{
    public int TestId { get; init; }
    public required string Name { get; init; }
    public required string Subject { get; init; }
    public int NumberOfQuestions { get; init; }
}
