namespace Jahoot.WebApi.Settings;

/// <summary>
/// This is where we keep our JWT settings.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// This is the secret key we use to sign our tokens.
    /// </summary>
    public required string Secret
    {
        get;
        init
        {
            if (value.Length < 32)
            {
                throw new InvalidOperationException("Secret must be at least 32 characters long.");
            }

            field = value;
        }
    }

    /// <summary>
    /// This is who gives out the tokens.
    /// </summary>
    public required string Issuer { get; init; }
    /// <summary>
    /// This is who the tokens are for.
    /// </summary>
    public required string Audience { get; init; }
}
