using Jahoot.WebApi.Settings;

namespace Jahoot.WebApi.Tests.Settings;

public class EmailSettingsTests
{
    [Test]
    public void EmailSettings_Properties_AreSetCorrectly()
    {
        const string host = "smtp.example.com";
        const int port = 587;
        const string user = "smtp_user";
        const string password = "smtp_password";
        const string fromEmail = "noreply@example.com";
        const string fromName = "Jahoot App";

        EmailSettings settings = new()
        {
            Host = host,
            Port = port,
            User = user,
            Password = password,
            FromEmail = fromEmail,
            FromName = fromName
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(settings.Host, Is.EqualTo(host));
            Assert.That(settings.Port, Is.EqualTo(port));
            Assert.That(settings.User, Is.EqualTo(user));
            Assert.That(settings.Password, Is.EqualTo(password));
            Assert.That(settings.FromEmail, Is.EqualTo(fromEmail));
            Assert.That(settings.FromName, Is.EqualTo(fromName));
        }
    }
}
