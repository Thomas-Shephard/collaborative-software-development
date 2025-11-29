using MailKit.Net.Smtp;
using MimeKit;
using Jahoot.WebApi.Settings;
using MailKit.Security;

namespace Jahoot.WebApi.Services;

public class SmtpEmailService(EmailSettings settings, ISmtpClientFactory clientFactory) : IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        MimeMessage message = new();
        message.From.Add(new MailboxAddress(settings.FromName, settings.FromEmail));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        message.Body = new TextPart("html")
        {
            Text = body
        };

        using ISmtpClient client = clientFactory.Create();

        // For development/testing with self-signed certs, we might need to bypass validation,
        // but for now we stick to standard secure defaults.

        await client.ConnectAsync(settings.Host, settings.Port, settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

        if (!string.IsNullOrEmpty(settings.User))
        {
            await client.AuthenticateAsync(settings.User, settings.Password);
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
