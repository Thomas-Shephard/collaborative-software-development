using Jahoot.Core.Models;

namespace Jahoot.Core.Tests.Models;

public class EmailMessageTests
{
    [Test]
    public void EmailMessage_CanBeInitializedWithValues()
    {
        const string to = "test@example.com";
        const string subject = "Test Subject";
        const string title = "Test Title";
        const string body = "Test Body";

        EmailMessage emailMessage = new()
        {
            To = to,
            Subject = subject,
            Title = title,
            Body = body
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(emailMessage.To, Is.EqualTo(to));
            Assert.That(emailMessage.Subject, Is.EqualTo(subject));
            Assert.That(emailMessage.Title, Is.EqualTo(title));
            Assert.That(emailMessage.Body, Is.EqualTo(body));
        }
    }
}
