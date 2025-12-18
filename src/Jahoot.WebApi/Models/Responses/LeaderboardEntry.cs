namespace Jahoot.WebApi.Models.Responses;

public class LeaderboardEntry
{
    public int Rank { get; init; }
    public required string StudentName { get; init; }
    public long TotalScore { get; init; }
}
