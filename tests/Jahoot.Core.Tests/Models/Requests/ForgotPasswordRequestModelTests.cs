using System.ComponentModel.DataAnnotations;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Core.Tests.Models.Requests;

public class ForgotPasswordRequestModelTests
{
    private const string ValidEmail = "test@example.com";

    [Test]
    public void ForgotPasswordRequest_WithValidEmail_IsValid()
    {
        ForgotPasswordRequestModel requestModel = new()
        {
            Email = ValidEmail
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
    public void ForgotPasswordRequest_WithInvalidEmail_IsInvalid(string email)
    {
        ForgotPasswordRequestModel requestModel = new()
        {
            Email = email
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
}
