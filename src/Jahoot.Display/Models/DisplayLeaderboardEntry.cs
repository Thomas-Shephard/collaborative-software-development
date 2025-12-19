namespace Jahoot.Display.Models;

public class DisplayLeaderboardEntry
{
    public int Rank { get; init; }
    public required string StudentName { get; init; }
    public long TotalScore { get; init; }
}
