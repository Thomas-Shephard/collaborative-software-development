using System.ComponentModel.DataAnnotations;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Core.Tests.Models.Requests;

public class CreateLecturerRequestModelTests
{
    [Test]
    public void CreateLecturerRequest_WithValidData_IsValid()
    {
        CreateLecturerRequestModel model = new()
        {
            Email = "lecturer@example.com",
            Name = "Dr. Lecturer",
            Password = "StrongPassword1!",
            IsAdmin = true
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
    public void CreateLecturerRequest_InvalidEmail_IsInvalid(string? email)
    {
        CreateLecturerRequestModel model = new()
        {
            Email = email!,
            Name = "Dr. Lecturer",
            Password = "StrongPassword1!",
            IsAdmin = false
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
    [TestCase("A")]
    [TestCase(null)]
    public void CreateLecturerRequest_InvalidName_IsInvalid(string? name)
    {
        CreateLecturerRequestModel model = new()
        {
            Email = "lecturer@example.com",
            Name = name!,
            Password = "StrongPassword1!",
            IsAdmin = false
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
    public void CreateLecturerRequest_LongName_IsInvalid()
    {
        CreateLecturerRequestModel model = new()
        {
            Email = "lecturer@example.com",
            Name = new string('A', 71),
            Password = "StrongPassword1!",
            IsAdmin = false
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
    [TestCase("")]
    [TestCase("weak")]
    [TestCase(null)]
    public void CreateLecturerRequest_InvalidPassword_IsInvalid(string? password)
    {
        CreateLecturerRequestModel model = new()
        {
            Email = "lecturer@example.com",
            Name = "Dr. Lecturer",
            Password = password!,
            IsAdmin = false
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
}
