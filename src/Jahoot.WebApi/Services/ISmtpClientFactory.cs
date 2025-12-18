using MailKit.Net.Smtp;

namespace Jahoot.WebApi.Services;

public interface ISmtpClientFactory
{
    ISmtpClient Create();
}
