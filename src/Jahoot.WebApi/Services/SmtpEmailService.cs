using System.Net;
using MailKit.Net.Smtp;
using MimeKit;
using Jahoot.WebApi.Settings;
using MailKit.Security;

namespace Jahoot.WebApi.Services;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ISmtpClientFactory _clientFactory;
    private readonly string _template;

    public SmtpEmailService(EmailSettings settings, ISmtpClientFactory clientFactory)
    {
        _settings = settings;
        _clientFactory = clientFactory;

        string templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "EmailTemplate.html");
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Email template not found at {templatePath}");
        }

        _template = File.ReadAllText(templatePath);
    }

    public async Task SendEmailAsync(string to, string subject, string title, string body)
    {
        using MimeMessage message = new();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;

        string htmlBody = _template
            .Replace("{{Title}}", WebUtility.HtmlEncode(title))
            .Replace("{{Body}}", WebUtility.HtmlEncode(body));

        message.Body = new TextPart("html")
        {
            Text = htmlBody
        };

        using ISmtpClient client = _clientFactory.Create();

        await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_settings.User, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
