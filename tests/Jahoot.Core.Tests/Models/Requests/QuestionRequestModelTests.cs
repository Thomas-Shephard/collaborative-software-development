using System.ComponentModel.DataAnnotations;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Core.Tests.Models.Requests;

public class QuestionRequestModelTests
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
        QuestionRequestModel model = new()
        {
            Text = "Question?",
            Options =
            [
                new QuestionOptionRequestModel { OptionText = "A", IsCorrect = true },
                new QuestionOptionRequestModel { OptionText = "B", IsCorrect = false }
            ]
        };

        List<ValidationResult> results = ValidateModel(model);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void Validate_MissingText_ReturnsError()
    {
        QuestionRequestModel model = new()
        {
            Text = null!,
            Options = [new QuestionOptionRequestModel { OptionText = "A", IsCorrect = true }, new QuestionOptionRequestModel { OptionText = "B", IsCorrect = false }]
        };

        List<ValidationResult> results = ValidateModel(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item(nameof(QuestionRequestModel.Text)));
    }

    [Test]
    public void Validate_MissingOptions_ReturnsError()
    {
        QuestionRequestModel model = new()
        {
            Text = "Question?",
            Options = null!
        };

        List<ValidationResult> results = ValidateModel(model);

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item(nameof(QuestionRequestModel.Options)));
    }

    [Test]
    public void Validate_FewerThanTwoOptions_ReturnsError()
    {
        QuestionRequestModel model = new()
        {
            Text = "Question?",
            Options = [new QuestionOptionRequestModel { OptionText = "A", IsCorrect = true }]
        };

        List<ValidationResult> results = ValidateModel(model);

        Assert.That(results, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[0].MemberNames, Contains.Item(nameof(QuestionRequestModel.Options)));
            Assert.That(results[0].ErrorMessage, Does.Contain("at least 2 options"));
        }
    }

    [Test]
    public void Validate_NoCorrectOption_ReturnsValidationError()
    {
        QuestionRequestModel model = new()
        {
            Text = "Question?",
            Options =
            [
                new QuestionOptionRequestModel { OptionText = "A", IsCorrect = false },
                new QuestionOptionRequestModel { OptionText = "B", IsCorrect = false }
            ]
        };

        List<ValidationResult> results = ValidateModel(model);

        Assert.That(results, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(results[0].MemberNames, Contains.Item(nameof(QuestionRequestModel.Options)));
            Assert.That(results[0].ErrorMessage, Does.Contain("At least one option must be marked as correct"));
        }
    }
}
