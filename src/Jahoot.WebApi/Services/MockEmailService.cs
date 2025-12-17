using Jahoot.Core.Models;

namespace Jahoot.WebApi.Services;

public class MockEmailService : IEmailService
{
    public Task SendEmailAsync(EmailMessage message)
    {
        Console.WriteLine($"MockEmailService: Sending email to {message.To} with subject {message.Subject}");
        Console.WriteLine($"Title: {message.Title}");
        Console.WriteLine($"Body: {message.Body}");
        return Task.CompletedTask;
    }
}
