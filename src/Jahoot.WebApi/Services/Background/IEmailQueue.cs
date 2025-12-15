using Jahoot.Core.Models;

namespace Jahoot.WebApi.Services.Background;

public interface IEmailQueue
{
    ValueTask QueueBackgroundEmailAsync(EmailMessage message);
    ValueTask<EmailMessage> DequeueAsync(CancellationToken cancellationToken);
}
