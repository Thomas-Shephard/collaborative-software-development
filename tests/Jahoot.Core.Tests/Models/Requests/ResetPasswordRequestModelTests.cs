using System.ComponentModel.DataAnnotations;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Core.Tests.Models.Requests;

public class ResetPasswordRequestModelTests
{
    private const string ValidEmail = "test@example.com";
    private const string ValidToken = "123456";
    private const string ValidNewPassword = "NewSecurePassword123";

    [Test]
    public void ResetPasswordRequest_WithValidData_IsValid()
    {
        ResetPasswordRequestModel requestModel = new()
        {
            Email = ValidEmail,
            Token = ValidToken,
            NewPassword = ValidNewPassword
        };

        ValidationContext validationContext = new(requestModel);
        List<ValidationResult> validationResults = [];

        bool isValid = Validator.TryValidateObject(requestModel, validationContext, validationResults, true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.True);
            Assert.That(validationResults, Is.Empty);
        }
    }

    [Test]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("invalid-email")]
    public void ResetPasswordRequest_WithInvalidEmail_IsInvalid(string email)
    {
        ResetPasswordRequestModel requestModel = new()
        {
            Email = email,
            Token = ValidToken,
            NewPassword = ValidNewPassword
        };

        ValidationContext validationContext = new(requestModel);
        List<ValidationResult> validationResults = [];

        bool isValid = Validator.TryValidateObject(requestModel, validationContext, validationResults, true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(validationResults.Any(vr => vr.MemberNames.Contains("Email")), Is.True);
        }
    }

    [Test]
    [TestCase("")]
    [TestCase(" ")]
    public void ResetPasswordRequest_WithInvalidToken_IsInvalid(string token)
    {
        ResetPasswordRequestModel requestModel = new()
        {
            Email = ValidEmail,
            Token = token,
            NewPassword = ValidNewPassword
        };

        ValidationContext validationContext = new(requestModel);
        List<ValidationResult> validationResults = new();

        bool isValid = Validator.TryValidateObject(requestModel, validationContext, validationResults, true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(validationResults.Any(vr => vr.MemberNames.Contains("Token")), Is.True);
        }
    }

    [Test]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("1234567")] // Less than 8 characters
    public void ResetPasswordRequest_WithInvalidNewPassword_IsInvalid(string newPassword)
    {
        ResetPasswordRequestModel requestModel = new()
        {
            Email = ValidEmail,
            Token = ValidToken,
            NewPassword = newPassword
        };

        ValidationContext validationContext = new(requestModel);
        List<ValidationResult> validationResults = new();

        bool isValid = Validator.TryValidateObject(requestModel, validationContext, validationResults, true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(validationResults.Any(vr => vr.MemberNames.Contains("NewPassword")), Is.True);
        }
    }
}
