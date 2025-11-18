using System.ComponentModel.DataAnnotations;
using Jahoot.Core.Models;

namespace Jahoot.Core.Tests.Models;

[TestFixture]
public class LoginRequestTests
{
    private const string LoginRequestEmail = "test@example.com";
    private const string LoginRequestPassword = "password123";

    [Test]
    public void LoginRequest_WithValidData_IsValid()
    {
        LoginRequest loginRequest = new()
        {
            Email = LoginRequestEmail,
            Password = LoginRequestPassword
        };

        ValidationContext validationContext = new(loginRequest);
        List<ValidationResult> validationResults = [];

        bool isValid = Validator.TryValidateObject(loginRequest, validationContext, validationResults, true);

        Assert.That(isValid, Is.True);
    }

    [Test]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("invalid-email")]
    public void LoginRequest_WithInvalidEmail_IsInvalid(string email)
    {
        LoginRequest loginRequest = new()
        {
            Email = email,
            Password = LoginRequestPassword
        };

        ValidationContext validationContext = new(loginRequest);
        List<ValidationResult> validationResults = [];

        bool isValid = Validator.TryValidateObject(loginRequest, validationContext, validationResults, true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(validationResults.Any(vr => vr.MemberNames.Contains("Email")));
        }
    }

    [Test]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("1234567")]
    public void LoginRequest_WithInvalidPassword_IsInvalid(string password)
    {
        LoginRequest loginRequest = new()
        {
            Email = LoginRequestEmail,
            Password = password
        };

        ValidationContext validationContext = new(loginRequest);
        List<ValidationResult> validationResults = [];

        bool isValid = Validator.TryValidateObject(loginRequest, validationContext, validationResults, true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(validationResults.Any(vr => vr.MemberNames.Contains("Password")));
        }
    }
}
