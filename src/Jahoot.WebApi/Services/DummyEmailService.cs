namespace Jahoot.WebApi.Services;

public class DummyEmailService : IEmailService
{
    public Task SendEmailAsync(string to, string subject, string body)
    {
        return Task.CompletedTask;
    }
}
