using Jahoot.WebApi.Services;
using MailKit.Net.Smtp;
using NUnit.Framework;

namespace Jahoot.WebApi.Tests.Services;

public class SmtpClientFactoryTests
{
    [Test]
    public void Create_ReturnsNewSmtpClient()
    {
        // Arrange
        SmtpClientFactory factory = new();

        // Act
        using ISmtpClient client = factory.Create();

        // Assert
        Assert.That(client, Is.Not.Null);
        Assert.That(client, Is.InstanceOf<SmtpClient>());
    }
}
