using MailKit.Net.Smtp;

namespace Jahoot.WebApi.Services;

public class SmtpClientFactory : ISmtpClientFactory
{
    public ISmtpClient Create() => new SmtpClient();
}
