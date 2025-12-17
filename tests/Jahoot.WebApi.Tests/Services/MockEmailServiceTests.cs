using Jahoot.WebApi.Services;
using Jahoot.Core.Models;

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

        try
        {
            await using StringWriter sw = new();
            Console.SetOut(sw);

            await service.SendEmailAsync(new EmailMessage
            {
                To = to,
                Subject = subject,
                Title = title,
                Body = body
            });

            string output = sw.ToString();
            Assert.That(output, Does.Contain($"MockEmailService: Sending email to {to} with subject {subject}"));
        }
        finally
        {
            await using StreamWriter standardOut = new(Console.OpenStandardOutput());
            standardOut.AutoFlush = true;
            Console.SetOut(standardOut);
        }
    }
}
