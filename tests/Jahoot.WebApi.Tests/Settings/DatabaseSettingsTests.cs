using Jahoot.WebApi.Settings;

namespace Jahoot.WebApi.Tests.Settings;

public class DatabaseSettingsTests
{
    private const string Host = "127.0.0.1";
    private const string Name = "test_db";
    private const string User = "test_user";
    private const string Password = "test_password";

    [Test]
    public void DatabaseSettings_ConnectionString_ReturnsCorrectFormat()
    {
        DatabaseSettings settings = new()
        {
            Host = Host,
            Name = Name,
            User = User,
            Password = Password
        };

        string connectionString = settings.ConnectionString;

        Assert.That(connectionString, Is.EqualTo($"Server={Host};Port=3306;Database={Name};User={User};Password={Password}"));
    }

    [Test]
    public void DatabaseSettings_Properties_AreSetCorrectly()
    {
        DatabaseSettings settings = new()
        {
            Host = Host,
            Name = Name,
            User = User,
            Password = Password
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(settings.Host, Is.EqualTo(Host));
            Assert.That(settings.Name, Is.EqualTo(Name));
            Assert.That(settings.User, Is.EqualTo(User));
            Assert.That(settings.Password, Is.EqualTo(Password));
        }
    }
}
