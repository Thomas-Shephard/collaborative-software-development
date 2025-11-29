using Jahoot.WebApi.Settings;
using NUnit.Framework;

namespace Jahoot.WebApi.Tests.Settings;

[TestFixture]
public class DatabaseSettingsTests
{
    [Test]
    public void DatabaseSettings_ConnectionString_ReturnsCorrectFormat()
    {
        // Arrange
        DatabaseSettings settings = new()
        {
            Host = "localhost",
            Name = "jahoot_db",
            User = "admin",
            Password = "secure_password"
        };

        // Act
        string connectionString = settings.ConnectionString;

        // Assert
        Assert.That(connectionString, Is.EqualTo("Server=localhost;Port=3306;Database=jahoot_db;User=admin;Password=secure_password"));
    }

    [Test]
    public void DatabaseSettings_Properties_AreSetCorrectly()
    {
        // Arrange
        string host = "127.0.0.1";
        string name = "test_db";
        string user = "test_user";
        string password = "test_password";

        // Act
        DatabaseSettings settings = new()
        {
            Host = host,
            Name = name,
            User = user,
            Password = password
        };

        // Assert
        Assert.That(settings.Host, Is.EqualTo(host));
        Assert.That(settings.Name, Is.EqualTo(name));
        Assert.That(settings.User, Is.EqualTo(user));
        Assert.That(settings.Password, Is.EqualTo(password));
    }
}
