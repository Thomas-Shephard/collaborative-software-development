using System.ComponentModel.DataAnnotations;
using System.Net;
using MailKit.Net.Smtp;
using MimeKit;
using Jahoot.WebApi.Settings;
using MailKit.Security;
using Jahoot.Core.Models;

namespace Jahoot.WebApi.Services;

public class SmtpEmailService : IEmailService
{
    private const string TemplateDirectory = "Templates";
    private const string TemplateFileName = "EmailTemplate.html";

    private readonly EmailSettings _settings;
    private readonly ISmtpClientFactory _clientFactory;
    private readonly string _template;

    public SmtpEmailService(EmailSettings settings, ISmtpClientFactory clientFactory)
    {
        _settings = settings;
        _clientFactory = clientFactory;

        string templatePath = Path.Combine(AppContext.BaseDirectory, TemplateDirectory, TemplateFileName);
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Email template not found at {templatePath}");
        }

        _template = File.ReadAllText(templatePath);
    }

    public async Task SendEmailAsync(EmailMessage message)
    {
        if (!new EmailAddressAttribute().IsValid(message.To))
        {
            throw new ArgumentException("Email address is not valid", nameof(message.To));
        }

        using MimeMessage mimeMessage = new();
        mimeMessage.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        mimeMessage.To.Add(new MailboxAddress("", message.To));
        mimeMessage.Subject = message.Subject;

        string htmlBody = _template
            .Replace("{{Title}}", WebUtility.HtmlEncode(message.Title))
            .Replace("{{Body}}", WebUtility.HtmlEncode(message.Body));

        mimeMessage.Body = new TextPart("html")
        {
            Text = htmlBody
        };

        using ISmtpClient client = _clientFactory.Create();

        await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_settings.User, _settings.Password);
        await client.SendAsync(mimeMessage);
        await client.DisconnectAsync(true);
    }
}
