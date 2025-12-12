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

    private string _templatePath;

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
            FromName = "Test Sender"
        };

        string templatesDir = Path.Combine(AppContext.BaseDirectory, "Templates");
        Directory.CreateDirectory(templatesDir);
        _templatePath = Path.Combine(templatesDir, "EmailTemplate.html");
        File.WriteAllText(_templatePath, "<html>{{Title}} - {{Body}}</html>");

        _service = new SmtpEmailService(_settings, _clientFactoryMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_templatePath))
        {
            File.Delete(_templatePath);
        }

        // Clean up directory if empty
        string? directory = Path.GetDirectoryName(_templatePath);
        if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any())
        {
            Directory.Delete(directory);
        }
    }

    [Test]
    public void Constructor_ThrowsFileNotFoundException_WhenTemplateIsMissing()
    {
        // Arrange
        if (File.Exists(_templatePath))
        {
            File.Delete(_templatePath);
        }

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => _ = new SmtpEmailService(_settings, _clientFactoryMock.Object));
    }

    [Test]
    public async Task SendEmailAsync_SendsEmailCorrectly()
    {
        // Arrange
        const string to = "recipient@example.com";
        const string subject = "Test Subject";
        const string heading = "Test Heading";
        const string body = "Test Body";

        string? capturedTo = null;
        string? capturedSubject = null;
        string? capturedHtmlBody = null;

        _clientMock.Setup(c => c.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()))
            .Callback<MimeMessage, CancellationToken, ITransferProgress>((m, _, _) =>
            {
                capturedTo = ((MailboxAddress)m.To[0]).Address;
                capturedSubject = m.Subject;
                capturedHtmlBody = m.HtmlBody;
            })
            .Returns(Task.FromResult(string.Empty));

        // Act
        await _service.SendEmailAsync(to, subject, heading, body);

        // Assert
        _clientMock.Verify(c => c.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, It.IsAny<CancellationToken>()), Times.Once);
        _clientMock.Verify(c => c.AuthenticateAsync(_settings.User, _settings.Password, It.IsAny<CancellationToken>()), Times.Once);

        _clientMock.Verify(c => c.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()), Times.Once);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(capturedTo, Is.EqualTo(to));
            Assert.That(capturedSubject, Is.EqualTo(subject));
            Assert.That(capturedHtmlBody, Does.Contain(body));
            Assert.That(capturedHtmlBody, Does.Contain(heading));
        }

        _clientMock.Verify(c => c.DisconnectAsync(true, It.IsAny<CancellationToken>()), Times.Once);
        _clientMock.Verify(c => c.Dispose(), Times.Once);
    }
}
