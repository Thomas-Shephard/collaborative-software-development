namespace Jahoot.WebApi.Settings;

public class LoginAttemptSettings
{
    public required int MaxFailedAttempts { get; init; }

    public required TimeSpan InitialLockoutDuration { get; init; }

    public required TimeSpan IncrementalLockoutDuration { get; init; }

    public required TimeSpan FailedAttemptResetInterval { get; init; }

    public required TimeSpan CleanupInterval { get; init; }
}
