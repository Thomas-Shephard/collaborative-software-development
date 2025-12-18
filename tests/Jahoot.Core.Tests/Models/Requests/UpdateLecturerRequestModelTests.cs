using System.ComponentModel.DataAnnotations;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Core.Tests.Models.Requests;

public class UpdateLecturerRequestModelTests
{
    [Test]
    public void UpdateLecturerRequest_WithValidData_IsValid()
    {
        UpdateLecturerRequestModel model = new()
        {
            Email = "lecturer@example.com",
            Name = "Dr. Lecturer",
            IsAdmin = true,
            IsDisabled = false
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
    public void UpdateLecturerRequest_InvalidEmail_IsInvalid(string? email)
    {
        UpdateLecturerRequestModel model = new()
        {
            Email = email!,
            Name = "Dr. Lecturer",
            IsAdmin = false,
            IsDisabled = false
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
    public void UpdateLecturerRequest_InvalidName_IsInvalid(string? name)
    {
        UpdateLecturerRequestModel model = new()
        {
            Email = "lecturer@example.com",
            Name = name!,
            IsAdmin = false,
            IsDisabled = false
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
    public void UpdateLecturerRequest_LongName_IsInvalid()
    {
        UpdateLecturerRequestModel model = new()
        {
            Email = "lecturer@example.com",
            Name = new string('A', 71),
            IsAdmin = false,
            IsDisabled = false
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
