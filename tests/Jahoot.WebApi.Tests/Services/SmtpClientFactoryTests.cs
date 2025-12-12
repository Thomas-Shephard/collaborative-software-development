using Jahoot.WebApi.Services;
using MailKit.Net.Smtp;

namespace Jahoot.WebApi.Tests.Services;

public class SmtpClientFactoryTests
{
    [Test]
    public void Create_ReturnsNewSmtpClient()
    {
        SmtpClientFactory factory = new();

        using ISmtpClient client = factory.Create();

        Assert.That(client, Is.Not.Null);
        Assert.That(client, Is.InstanceOf<SmtpClient>());
    }
}
