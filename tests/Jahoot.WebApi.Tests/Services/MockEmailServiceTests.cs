using Jahoot.WebApi.Services;
using NUnit.Framework;

namespace Jahoot.WebApi.Tests.Services;

[TestFixture]
public class MockEmailServiceTests
{
    [Test]
    public async Task SendEmailAsync_WritesToConsole()
    {
        // Arrange
        MockEmailService service = new();
        string to = "test@example.com";
        string subject = "Test Subject";
        string title = "Test Title";
        string body = "Test Body";

        await using StringWriter sw = new();
        Console.SetOut(sw);

        // Act
        await service.SendEmailAsync(to, subject, title, body);

        // Assert
        string output = sw.ToString();
        Assert.That(output, Does.Contain($"MockEmailService: Sending email to {to} with subject {subject}"));
        Assert.That(output, Does.Contain($"Title: {title}"));
        Assert.That(output, Does.Contain($"Body: {body}"));

        // Cleanup
        StreamWriter standardOut = new(Console.OpenStandardOutput());
        standardOut.AutoFlush = true;
        Console.SetOut(standardOut);
    }
}
