using System.ComponentModel.DataAnnotations;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Core.Tests.Models.Requests;

[TestFixture]
public class StudentRegistrationRequestModelTests
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
        var model = new StudentRegistrationRequestModel
        {
            Email = "test@example.com",
            Password = "password123",
            Name = "John Doe"
        };

        var results = ValidateModel(model);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void Validate_MissingEmail_ReturnsError()
    {
        var model = new StudentRegistrationRequestModel
        {
            Email = null!,
            Password = "password123",
            Name = "John Doe"
        };

        var results = ValidateModel(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item(nameof(StudentRegistrationRequestModel.Email)));
    }

    [Test]
    public void Validate_InvalidEmail_ReturnsError()
    {
        var model = new StudentRegistrationRequestModel
        {
            Email = "invalid-email",
            Password = "password123",
            Name = "John Doe"
        };

        var results = ValidateModel(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item(nameof(StudentRegistrationRequestModel.Email)));
    }

    [Test]
    public void Validate_MissingPassword_ReturnsError()
    {
        var model = new StudentRegistrationRequestModel
        {
            Email = "test@example.com",
            Password = null!,
            Name = "John Doe"
        };

        var results = ValidateModel(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item(nameof(StudentRegistrationRequestModel.Password)));
    }

    [Test]
    public void Validate_ShortPassword_ReturnsError()
    {
        var model = new StudentRegistrationRequestModel
        {
            Email = "test@example.com",
            Password = "short",
            Name = "John Doe"
        };

        var results = ValidateModel(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item(nameof(StudentRegistrationRequestModel.Password)));
    }

    [Test]
    public void Validate_MissingName_ReturnsError()
    {
        var model = new StudentRegistrationRequestModel
        {
            Email = "test@example.com",
            Password = "password123",
            Name = null!
        };

        var results = ValidateModel(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item(nameof(StudentRegistrationRequestModel.Name)));
    }

    [Test]
    public void Validate_LongName_ReturnsError()
    {
        var model = new StudentRegistrationRequestModel
        {
            Email = "test@example.com",
            Password = "password123",
            Name = new string('A', 71)
        };

        var results = ValidateModel(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item(nameof(StudentRegistrationRequestModel.Name)));
    }
}