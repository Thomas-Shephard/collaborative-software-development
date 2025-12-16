using Jahoot.Core.Models;

namespace Jahoot.WebApi.Services;

public interface IEmailService
{
    Task SendEmailAsync(EmailMessage message);
}
