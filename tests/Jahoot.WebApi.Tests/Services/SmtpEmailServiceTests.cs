using Jahoot.WebApi.Services;
using Jahoot.WebApi.Settings;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Moq;
using NUnit.Framework;

namespace Jahoot.WebApi.Tests.Services;

[TestFixture]
public class SmtpEmailServiceTests
{
    private Mock<ISmtpClientFactory> _clientFactoryMock;
    private Mock<ISmtpClient> _clientMock;
    private EmailSettings _settings;
    private SmtpEmailService _service;

    [SetUp]
    public void Setup()
    {
        _clientFactoryMock = new Mock<ISmtpClientFactory>();
        _clientMock = new Mock<ISmtpClient>();
        
        _clientFactoryMock.Setup(x => x.Create()).Returns(_clientMock.Object);

        _settings = new EmailSettings
        {
            Host = "localhost",
            Port = 1025,
            User = "user",
            Password = "password",
            FromEmail = "test@example.com",
            FromName = "Test Sender",
            EnableSsl = true
        };

        _service = new SmtpEmailService(_settings, _clientFactoryMock.Object);
    }

    [Test]
    public async Task SendEmailAsync_SendsEmailCorrectly()
    {
        // Arrange
        string to = "recipient@example.com";
        string subject = "Test Subject";
        string body = "<p>Test Body</p>";

        // Act
        await _service.SendEmailAsync(to, subject, body);

        // Assert
        _clientMock.Verify(c => c.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, It.IsAny<CancellationToken>()), Times.Once);
        _clientMock.Verify(c => c.AuthenticateAsync(_settings.User, _settings.Password, It.IsAny<CancellationToken>()), Times.Once);
        
        _clientMock.Verify(c => c.SendAsync(It.Is<MimeMessage>(m => 
            m.To.Count == 1 && 
            ((MailboxAddress)m.To[0]).Address == to &&
            m.Subject == subject &&
            m.HtmlBody == body
        ), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()), Times.Once);
        
        _clientMock.Verify(c => c.DisconnectAsync(true, It.IsAny<CancellationToken>()), Times.Once);
        _clientMock.Verify(c => c.Dispose(), Times.Once);
    }
}
