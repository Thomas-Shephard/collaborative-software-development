namespace Jahoot.WebApi.Models.Responses;

public class CompletedTestResponse
{
    public int TestId { get; init; }
    public required string TestName { get; init; }
    public required string SubjectName { get; init; }
    public DateTime CompletedDate { get; init; }
    public double ScorePercentage { get; init; }
    public int TotalPoints { get; init; }
}
