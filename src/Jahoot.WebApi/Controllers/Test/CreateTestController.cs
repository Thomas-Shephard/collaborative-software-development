using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Test;

[Route("api/test")]
[ApiController]
[Tags("Test")]
public class CreateTestController(ITestRepository testRepository, ISubjectRepository subjectRepository) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = nameof(Role.Lecturer))]
    public async Task<IActionResult> CreateTest([FromBody] TestRequestModel requestModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Core.Models.Subject? subject = await subjectRepository.GetSubjectByIdAsync(requestModel.SubjectId);
        if (subject is null)
        {
            return BadRequest($"Subject with ID {requestModel.SubjectId} does not exist.");
        }

        List<Question> questions = requestModel.Questions.Select(question => new Question
        {
            Text = question.Text,
            Options = question.Options.Select(questionOption => new QuestionOption
            {
                OptionText = questionOption.OptionText,
                IsCorrect = questionOption.IsCorrect
            }).ToList().AsReadOnly()
        }).ToList();

        Core.Models.Test test = new()
        {
            SubjectId = requestModel.SubjectId,
            Name = requestModel.Name,
            NumberOfQuestions = requestModel.NumberOfQuestions,
            Questions = questions.AsReadOnly()
        };

        await testRepository.CreateTestAsync(test);
        return Created();
    }
}
