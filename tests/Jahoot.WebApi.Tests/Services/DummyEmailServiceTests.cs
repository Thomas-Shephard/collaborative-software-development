using Jahoot.WebApi.Services;

namespace Jahoot.WebApi.Tests.Services;

public class DummyEmailServiceTests
{
    [Test]
    public async Task SendEmailAsync_ReturnsCompletedTask()
    {
        DummyEmailService service = new();

        Task task = service.SendEmailAsync("test@example.com", "Subject", "Body");
        await task;

        Assert.That(task.IsCompletedSuccessfully, Is.True);
    }
}
