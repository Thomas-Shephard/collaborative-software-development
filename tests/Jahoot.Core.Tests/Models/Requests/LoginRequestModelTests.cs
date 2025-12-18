using System.ComponentModel.DataAnnotations;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Core.Tests.Models.Requests;

public class LoginRequestModelTests
{
    private const string LoginRequestEmail = "test@example.com";
    private const string LoginRequestPassword = "StrongPassword1!";

    [Test]
    public void LoginRequest_WithValidData_IsValid()
    {
        LoginRequestModel loginRequestModel = new()
        {
            Email = LoginRequestEmail,
            Password = LoginRequestPassword
        };

        ValidationContext validationContext = new(loginRequestModel);
        List<ValidationResult> validationResults = [];

        bool isValid = Validator.TryValidateObject(loginRequestModel, validationContext, validationResults, true);

        Assert.That(isValid, Is.True);
    }

    [Test]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("invalid-email")]
    public void LoginRequest_WithInvalidEmail_IsInvalid(string email)
    {
        LoginRequestModel loginRequestModel = new()
        {
            Email = email,
            Password = LoginRequestPassword
        };

        ValidationContext validationContext = new(loginRequestModel);
        List<ValidationResult> validationResults = [];

        bool isValid = Validator.TryValidateObject(loginRequestModel, validationContext, validationResults, true);

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
        LoginRequestModel loginRequestModel = new()
        {
            Email = LoginRequestEmail,
            Password = password
        };

        ValidationContext validationContext = new(loginRequestModel);
        List<ValidationResult> validationResults = [];

        bool isValid = Validator.TryValidateObject(loginRequestModel, validationContext, validationResults, true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(validationResults.Any(vr => vr.MemberNames.Contains("Password")));
        }
    }
}
