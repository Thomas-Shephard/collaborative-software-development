namespace Jahoot.WebApi.Settings;

public class ScoringSettings
{
    public required int PointsPerCorrectAnswer { get; init; }
    public required int PointsPerIncorrectAnswer { get; init; }
}
