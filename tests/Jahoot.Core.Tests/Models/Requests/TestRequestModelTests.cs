using System.ComponentModel.DataAnnotations;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Core.Tests.Models.Requests;

public class TestRequestModelTests
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
        TestRequestModel model = new()
        {
            SubjectId = 1,
            Name = "Test Name",
            NumberOfQuestions = 1,
            Questions =
            [
                new QuestionRequestModel
                {
                    Text = "Q1",
                    Options = [new QuestionOptionRequestModel { OptionText = "A", IsCorrect = true }, new QuestionOptionRequestModel { OptionText = "B", IsCorrect = false }]
                }
            ]
        };

        List<ValidationResult> results = ValidateModel(model);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void Validate_MissingName_ReturnsError()
    {
        TestRequestModel model = new()
        {
            SubjectId = 1,
            Name = null!,
            Questions = []
        };

        List<ValidationResult> results = ValidateModel(model);

        Assert.That(results.Any(r => r.MemberNames.Contains(nameof(TestRequestModel.Name))), Is.True);
    }

    [Test]
    public void Validate_MissingQuestions_ReturnsError()
    {
        TestRequestModel model = new()
        {
            SubjectId = 1,
            Name = "Test",
            Questions = null!
        };

        List<ValidationResult> results = ValidateModel(model);

        Assert.That(results.Any(r => r.MemberNames.Contains(nameof(TestRequestModel.Questions))), Is.True);
    }

    [Test]
    public void Validate_EmptyQuestionsList_ReturnsError()
    {
        TestRequestModel model = new()
        {
            SubjectId = 1,
            Name = "Test",
            Questions = []
        };

        List<ValidationResult> results = ValidateModel(model);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(TestRequestModel.Questions))), Is.True);
            Assert.That(results.Any(r => r.ErrorMessage!.Contains("at least 1 question")), Is.True);
        }
    }

    [Test]
    public void Validate_NumberOfQuestionsExceedsCount_ReturnsError()
    {
        TestRequestModel model = new()
        {
            SubjectId = 1,
            Name = "Test",
            NumberOfQuestions = 2,
            Questions =
            [
                new QuestionRequestModel
                {
                    Text = "Q1",
                    Options = [new QuestionOptionRequestModel { OptionText = "A", IsCorrect = true }]
                }
            ]
        };

        List<ValidationResult> results = ValidateModel(model);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results.Any(r => r.MemberNames.Contains(nameof(TestRequestModel.NumberOfQuestions))), Is.True);
            Assert.That(results.Any(r => r.ErrorMessage!.Contains("cannot exceed")), Is.True);
        }
    }

    [Test]
    public void Validate_NumberOfQuestionsLessThanOne_ReturnsError()
    {
        TestRequestModel model = new()
        {
            SubjectId = 1,
            Name = "Test",
            NumberOfQuestions = 0,
            Questions =
            [
                new QuestionRequestModel
                {
                    Text = "Q1",
                    Options = [new QuestionOptionRequestModel { OptionText = "A", IsCorrect = true }]
                }
            ]
        };

        List<ValidationResult> results = ValidateModel(model);

        Assert.That(results.Any(r => r.MemberNames.Contains(nameof(TestRequestModel.NumberOfQuestions))), Is.True);
    }
}
