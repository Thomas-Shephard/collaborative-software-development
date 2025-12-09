using System.ComponentModel.DataAnnotations;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Core.Tests.Models.Requests;

public class StudentRegistrationRequestModelTests
{
    [Test]
    public void StudentRegistrationRequest_WithValidData_IsValid()
    {
        StudentRegistrationRequestModel model = new()
        {
            Email = "test@example.com",
            Password = "StrongPassword1!",
            Name = "Test Student"
        };

        ValidationContext context = new(model);
        List<ValidationResult> results = [];
        bool isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.That(isValid, Is.True);
    }

    [Test]
    [TestCase("")]
    [TestCase("invalid-email")]
    [TestCase(null)]
    public void StudentRegistrationRequest_InvalidEmail_IsInvalid(string?email)
    {
        StudentRegistrationRequestModel model = new()
        {
            Email = email!,
            Password = "password123",
            Name = "Test Student"
        };

        ValidationContext context = new(model);
        List<ValidationResult> results = [];
        bool isValid = Validator.TryValidateObject(model, context, results, true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains("Email")));
        }
    }

    [Test]
    [TestCase("")]
    [TestCase("short")]
    [TestCase(null)]
    public void StudentRegistrationRequest_InvalidPassword_IsInvalid(string?password)
    {
        StudentRegistrationRequestModel model = new()
        {
            Email = "test@example.com",
            Password = password!,
            Name = "Test Student"
        };

        ValidationContext context = new(model);
        List<ValidationResult> results = [];
        bool isValid = Validator.TryValidateObject(model, context, results, true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains("Password")));
        }
    }

    [Test]
    [TestCase("")]
    [TestCase(null)]
    public void StudentRegistrationRequest_InvalidName_IsInvalid(string?name)
    {
        StudentRegistrationRequestModel model = new()
        {
            Email = "test@example.com",
            Password = "password123",
            Name = name!
        };

        ValidationContext context = new(model);
        List<ValidationResult> results = [];
        bool isValid = Validator.TryValidateObject(model, context, results, true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains("Name")));
        }
    }

    [Test]
    public void StudentRegistrationRequest_LongName_IsInvalid()
    {
        StudentRegistrationRequestModel model = new()
        {
            Email = "test@example.com",
            Password = "password123",
            Name = new string('a', 71) // MaxLength is 70
        };

        ValidationContext context = new(model);
        List<ValidationResult> results = [];
        bool isValid = Validator.TryValidateObject(model, context, results, true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(results.Any(r => r.MemberNames.Contains("Name")));
        }
    }
}
