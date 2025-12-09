using System.ComponentModel.DataAnnotations;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Core.Tests.Models.Requests;

[TestFixture]
public class ExistingUserRegistrationRequestModelTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }

    [Test]
    public void Validate_ValidModel_ReturnsNoErrors()
    {
        var model = new ExistingUserRegistrationRequestModel
        {
            Email = "test@example.com",
            Password = "StrongPassword1!"
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void Validate_MissingEmail_ReturnsError()
    {
        var model = new ExistingUserRegistrationRequestModel
        {
            Email = null!,
            Password = "password123"
        };

        var results = ValidateModel(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item(nameof(ExistingUserRegistrationRequestModel.Email)));
    }

    [Test]
    public void Validate_InvalidEmail_ReturnsError()
    {
        var model = new ExistingUserRegistrationRequestModel
        {
            Email = "invalid-email",
            Password = "password123"
        };

        var results = ValidateModel(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item(nameof(ExistingUserRegistrationRequestModel.Email)));
    }

    [Test]
    public void Validate_MissingPassword_ReturnsError()
    {
        var model = new ExistingUserRegistrationRequestModel
        {
            Email = "test@example.com",
            Password = null!
        };

        var results = ValidateModel(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item(nameof(ExistingUserRegistrationRequestModel.Password)));
    }
}