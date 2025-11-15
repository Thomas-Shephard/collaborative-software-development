using Jahoot.WebApi.Settings;

namespace Jahoot.WebApi.Tests.Settings;

public class TokenDenySettingsTests
{
    [Test]
    public void TokenDenySettings_CanBeInstantiated_WithValidProperties()
    {
        TimeSpan cleanupInterval = TimeSpan.FromHours(1);

        TokenDenySettings settings = new()
        {
            CleanupInterval = cleanupInterval
        };

        Assert.That(settings.CleanupInterval, Is.EqualTo(cleanupInterval));
    }

    [Test]
    public void TokenDenySettings_CleanupIntervalIsZero_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = new TokenDenySettings
            {
                CleanupInterval = TimeSpan.Zero
            };
        });
    }
}
