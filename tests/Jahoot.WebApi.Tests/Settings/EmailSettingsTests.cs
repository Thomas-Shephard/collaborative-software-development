using Jahoot.WebApi.Settings;
using NUnit.Framework;

namespace Jahoot.WebApi.Tests.Settings;

[TestFixture]
public class EmailSettingsTests
{
    [Test]
    public void EmailSettings_Properties_AreSetCorrectly()
    {
        // Arrange
        string host = "smtp.example.com";
        int port = 587;
        string user = "smtp_user";
        string password = "smtp_password";
        string fromEmail = "noreply@example.com";
        string fromName = "Jahoot App";

        // Act
        EmailSettings settings = new()
        {
            Host = host,
            Port = port,
            User = user,
            Password = password,
            FromEmail = fromEmail,
            FromName = fromName
        };

        // Assert
        Assert.That(settings.Host, Is.EqualTo(host));
        Assert.That(settings.Port, Is.EqualTo(port));
        Assert.That(settings.User, Is.EqualTo(user));
        Assert.That(settings.Password, Is.EqualTo(password));
        Assert.That(settings.FromEmail, Is.EqualTo(fromEmail));
        Assert.That(settings.FromName, Is.EqualTo(fromName));
    }
}
