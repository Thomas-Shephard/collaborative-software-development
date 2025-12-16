using Jahoot.Core.Models;
using Jahoot.WebApi.Services;
using Jahoot.WebApi.Services.Background;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Jahoot.WebApi.Tests.Services.Background;

public class EmailBackgroundServiceTests
{
    private Mock<IEmailQueue> _queueMock;
    private Mock<IServiceScopeFactory> _scopeFactoryMock;
    private Mock<IServiceScope> _scopeMock;
    private Mock<IServiceProvider> _serviceProviderMock;
    private Mock<IEmailService> _emailServiceMock;
    private EmailBackgroundService _service;

    [SetUp]
    public void Setup()
    {
        _queueMock = new Mock<IEmailQueue>();
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _emailServiceMock = new Mock<IEmailService>();

        _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(_scopeMock.Object);
        _scopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IEmailService))).Returns(_emailServiceMock.Object);

        _service = new EmailBackgroundService(_queueMock.Object, _scopeFactoryMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _service.Dispose();
    }

    [Test]
    public async Task ExecuteAsync_ProcessesMessage()
    {
        EmailMessage message = new()
        {
            To = "test@example.com",
            Subject = "Test",
            Title = "Test",
            Body = "Body"
        };

        CancellationTokenSource cts = new();

        _queueMock.SetupSequence(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<EmailMessage>(message))
            .Returns(new ValueTask<EmailMessage>(Task.FromException<EmailMessage>(new OperationCanceledException())));

        await _service.StartAsync(cts.Token);
        await Task.Delay(100, cts.Token);
        await _service.StopAsync(cts.Token);

        _emailServiceMock.Verify(x => x.SendEmailAsync(message), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ContinuesProcessing_OnException()
    {
        EmailMessage failMessage = new() { To = "fail", Subject = "Fail", Title = "Fail", Body = "Fail" };
        EmailMessage successMessage = new() { To = "success", Subject = "Success", Title = "Success", Body = "Success" };

        CancellationTokenSource cts = new();

        _queueMock.SetupSequence(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<EmailMessage>(failMessage))
            .Returns(new ValueTask<EmailMessage>(successMessage))
            .Returns(new ValueTask<EmailMessage>(Task.FromException<EmailMessage>(new OperationCanceledException())));

        _emailServiceMock.Setup(x => x.SendEmailAsync(failMessage)).ThrowsAsync(new Exception("Processing failed"));
        _emailServiceMock.Setup(x => x.SendEmailAsync(successMessage)).Returns(Task.CompletedTask);

        await _service.StartAsync(cts.Token);
        await Task.Delay(100, cts.Token);
        await _service.StopAsync(cts.Token);

        _emailServiceMock.Verify(x => x.SendEmailAsync(failMessage), Times.Once);
        _emailServiceMock.Verify(x => x.SendEmailAsync(successMessage), Times.Once);
    }
}
