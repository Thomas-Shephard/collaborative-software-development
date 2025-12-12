using Jahoot.WebApi.Services;

namespace Jahoot.WebApi.Tests.Services;

public class MockEmailServiceTests
{
    [Test]
    public async Task SendEmailAsync_WritesToConsole()
    {
        MockEmailService service = new();
        const string to = "test@example.com";
        const string subject = "Test Subject";
        const string title = "Test Title";
        const string body = "Test Body";

        await using StringWriter sw = new();
        Console.SetOut(sw);

        await service.SendEmailAsync(to, subject, title, body);

        string output = sw.ToString();
        Assert.That(output, Does.Contain($"MockEmailService: Sending email to {to} with subject {subject}"));
        Assert.That(output, Does.Contain($"Title: {title}"));
        Assert.That(output, Does.Contain($"Body: {body}"));

        StreamWriter standardOut = new(Console.OpenStandardOutput());
        standardOut.AutoFlush = true;
        Console.SetOut(standardOut);
    }
}
