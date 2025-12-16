using Jahoot.Core.Models;
using Jahoot.WebApi.Services.Background;

namespace Jahoot.WebApi.Tests.Services.Background;

public class EmailQueueTests
{
    [Test]
    public async Task QueueBackgroundEmailAsync_AddsMessageToQueue()
    {
        EmailQueue queue = new();
        EmailMessage message = new()
        {
            To = "test@example.com",
            Subject = "Test",
            Title = "Test",
            Body = "Test Body"
        };
        CancellationTokenSource cts = new();

        await queue.QueueBackgroundEmailAsync(message);
        EmailMessage dequeuedMessage = await queue.DequeueAsync(cts.Token);

        Assert.That(dequeuedMessage, Is.SameAs(message));
    }

    [Test]
    public void DequeueAsync_ThrowsOperationCanceledException_WhenCancelled()
    {
        EmailQueue queue = new();
        CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () => await queue.DequeueAsync(cts.Token));
    }
}
