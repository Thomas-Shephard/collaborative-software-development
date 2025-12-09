using System.ComponentModel.DataAnnotations;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Core.Tests.Models.Requests;

public class UpdateSubjectRequestModelTests
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
        UpdateSubjectRequestModel model = new()
        {
            Name = "Mathematics",
            IsActive = true
        };

        List<ValidationResult> results = ValidateModel(model);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void Validate_MissingName_ReturnsError()
    {
        UpdateSubjectRequestModel model = new()
        {
            Name = null!,
            IsActive = true
        };

        List<ValidationResult> results = ValidateModel(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item(nameof(UpdateSubjectRequestModel.Name)));
    }

    [Test]
    public void Validate_LongName_ReturnsError()
    {
        UpdateSubjectRequestModel model = new()
        {
            Name = new string('A', 256),
            IsActive = true
        };

        List<ValidationResult> results = ValidateModel(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item(nameof(UpdateSubjectRequestModel.Name)));
    }
}
