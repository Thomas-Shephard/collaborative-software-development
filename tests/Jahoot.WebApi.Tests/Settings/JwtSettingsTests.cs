using Jahoot.WebApi.Settings;

namespace Jahoot.WebApi.Tests.Settings;

public class JwtSettingsTests
{
    private const string Issuer = "TestIssuer";
    private const string Audience = "TestAudience";

    [Test]
    public void JwtSettings_ValidSecret_CanBeInstantiated()
    {
        const string validSecret = "a-very-secure-secret-key-that-is-long-enough-and-at-least-32-chars";

        JwtSettings settings = new()
        {
            Secret = validSecret,
            Issuer = Issuer,
            Audience = Audience
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(settings.Secret, Is.EqualTo(validSecret));
            Assert.That(settings.Issuer, Is.EqualTo(Issuer));
            Assert.That(settings.Audience, Is.EqualTo(Audience));
        }
    }

    [Test]
    public void JwtSettings_SecretTooShort_ThrowsInvalidOperationException()
    {
        const string shortSecret = "short"; // Less than 32 characters

        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = new JwtSettings
            {
                Secret = shortSecret,
                Issuer = Issuer,
                Audience = Audience
            };
        });
    }
}
