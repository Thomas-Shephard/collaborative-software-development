namespace Jahoot.WebApi.Models.Responses;

public class StudentStatisticsResponse
{
    public int TotalPoints { get; init; }
    public double AverageScorePercentage { get; init; }
    public IEnumerable<ScoreHistoryItem> ScoreHistory { get; init; } = [];
}

public class ScoreHistoryItem
{
    public DateTime Date { get; init; }
    public double ScorePercentage { get; init; }
}
