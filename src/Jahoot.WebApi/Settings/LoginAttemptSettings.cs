namespace Jahoot.WebApi.Settings;

public class LoginAttemptSettings
{
    public required int MaxFailedAttempts
    {
        get;
        init
        {
            if (value <= 0)
            {
                throw new InvalidOperationException($"{nameof(MaxFailedAttempts)} must be a positive integer.");
            }

            field = value;
        }
    }

    public required TimeSpan InitialLockoutDuration
    {
        get;
        init
        {
            if (value <= TimeSpan.Zero)
            {
                throw new InvalidOperationException($"{nameof(InitialLockoutDuration)} must be a positive TimeSpan.");
            }

            field = value;
        }
    }

    public required TimeSpan IncrementalLockoutDuration
    {
        get;
        init
        {
            if (value <= TimeSpan.Zero)
            {
                throw new InvalidOperationException($"{nameof(IncrementalLockoutDuration)} must be a positive TimeSpan.");
            }

            field = value;
        }
    }

    public required TimeSpan FailedAttemptResetInterval
    {
        get;
        init
        {
            if (value <= TimeSpan.Zero)
            {
                throw new InvalidOperationException($"{nameof(FailedAttemptResetInterval)} must be a positive TimeSpan.");
            }

            field = value;
        }
    }

    public required TimeSpan CleanupInterval
    {
        get;
        init
        {
            if (value <= TimeSpan.Zero)
            {
                throw new InvalidOperationException($"{nameof(CleanupInterval)} must be a positive TimeSpan.");
            }

            field = value;
        }
    }
}
