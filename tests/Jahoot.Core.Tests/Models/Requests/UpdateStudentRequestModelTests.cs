using System.ComponentModel.DataAnnotations;
using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Core.Tests.Models.Requests;

public class UpdateStudentRequestModelTests
{
    private static List<ValidationResult> ValidateModel(object model)
    {
        List<ValidationResult> validationResults = [];
        ValidationContext validationContext = new(model, null, null);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }

    [Test]
    public void Validate_ValidModel_ReturnsNoErrors()
    {
        UpdateStudentRequestModel model = new()
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            AccountStatus = StudentAccountStatus.Active,
            SubjectIds = [ 1 ]
        };

        List<ValidationResult> results = ValidateModel(model);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void Validate_MissingName_ReturnsError()
    {
        UpdateStudentRequestModel model = new()
        {
            Name = null!,
            Email = "john.doe@example.com",
            AccountStatus = StudentAccountStatus.Active,
            SubjectIds = []
        };

        List<ValidationResult> results = ValidateModel(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item(nameof(UpdateStudentRequestModel.Name)));
    }

    [Test]
    public void Validate_LongName_ReturnsError()
    {
        UpdateStudentRequestModel model = new()
        {
            Name = new string('A', 71), // MaxLength is 70
            Email = "john.doe@example.com",
            AccountStatus = StudentAccountStatus.Active,
            SubjectIds = [ 1, 2, 3 ]
        };

        List<ValidationResult> results = ValidateModel(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item(nameof(UpdateStudentRequestModel.Name)));
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("invalid-email")]
    public void Validate_InvalidEmail_ReturnsError(string? email)
    {
        UpdateStudentRequestModel model = new()
        {
            Name = "John Doe",
            Email = email!,
            AccountStatus = StudentAccountStatus.Active,
            SubjectIds = []
        };

        List<ValidationResult> results = ValidateModel(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item(nameof(UpdateStudentRequestModel.Email)));
    }

    [Test]
    public void Validate_DuplicateSubjectIds_ReturnsError()
    {
        UpdateStudentRequestModel model = new()
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            AccountStatus = StudentAccountStatus.Active,
            SubjectIds = [1, 1]
        };

        List<ValidationResult> results = ValidateModel(model);

        Assert.That(results, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[0].MemberNames, Contains.Item(nameof(UpdateStudentRequestModel.SubjectIds)));
            Assert.That(results[0].ErrorMessage, Does.Contain("must not contain duplicate elements"));
        }
    }
}
