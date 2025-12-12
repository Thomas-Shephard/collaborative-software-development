using System.ComponentModel.DataAnnotations;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Core.Tests.Models.Requests;

public class AssignStudentRoleRequestModelTests
{
    [Test]
    public void ExistingUserRegistrationRequest_WithValidData_IsValid()
    {
        AssignStudentRoleRequestModel model = new()
        {
            Email = "test@example.com",
            Password = "StrongPassword1!"
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
    public void ExistingUserRegistrationRequest_InvalidEmail_IsInvalid(string? email)
    {
        AssignStudentRoleRequestModel model = new()
        {
            Email = email!,
            Password = "password"
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
    [TestCase(null)]
    public void ExistingUserRegistrationRequest_InvalidPassword_IsInvalid(string? password)
    {
        AssignStudentRoleRequestModel model = new()
        {
            Email = "test@example.com",
            Password = password!
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
