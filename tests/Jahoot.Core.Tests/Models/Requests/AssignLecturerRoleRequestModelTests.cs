using System.ComponentModel.DataAnnotations;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Core.Tests.Models.Requests;

public class AssignLecturerRoleRequestModelTests
{
    [Test]
    public void AssignLecturerRoleRequest_WithValidData_IsValid()
    {
        AssignLecturerRoleRequestModel model = new()
        {
            Email = "lecturer@example.com",
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
    public void AssignLecturerRoleRequest_InvalidEmail_IsInvalid(string? email)
    {
        AssignLecturerRoleRequestModel model = new()
        {
            Email = email!,
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
}
