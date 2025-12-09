using System.ComponentModel.DataAnnotations;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Core.Tests.Models.Requests;

public class CreateSubjectRequestModelTests
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
        CreateSubjectRequestModel model = new()
        {
            Name = "Mathematics"
        };

        List<ValidationResult> results = ValidateModel(model);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void Validate_MissingName_ReturnsError()
    {
        CreateSubjectRequestModel model = new()
        {
            Name = null!
        };

        List<ValidationResult> results = ValidateModel(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item(nameof(CreateSubjectRequestModel.Name)));
    }

    [Test]
    public void Validate_LongName_ReturnsError()
    {
        CreateSubjectRequestModel model = new()
        {
            Name = new string('A', 256)
        };

        List<ValidationResult> results = ValidateModel(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item(nameof(CreateSubjectRequestModel.Name)));
    }
}
