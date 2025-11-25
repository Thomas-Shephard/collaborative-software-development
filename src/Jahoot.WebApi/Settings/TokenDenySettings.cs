namespace Jahoot.WebApi.Settings;

public class TokenDenySettings
{
    public required TimeSpan CleanupInterval { get; init; }
}
