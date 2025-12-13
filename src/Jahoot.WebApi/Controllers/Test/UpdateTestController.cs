using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Test;

[Route("api/test/{testId:int}")]
[ApiController]
[Tags("Test")]
public class UpdateTestController(ITestRepository testRepository, ISubjectRepository subjectRepository) : ControllerBase
{
    [HttpPut]
    [Authorize(Policy = nameof(Role.Lecturer))]
    public async Task<IActionResult> UpdateTest(int testId, [FromBody] TestRequestModel requestModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Core.Models.Test? test = await testRepository.GetTestByIdAsync(testId);
        if (test is null)
        {
            return NotFound();
        }

        Core.Models.Subject? subject = await subjectRepository.GetSubjectByIdAsync(requestModel.SubjectId);
        if (subject is null)
        {
            return BadRequest($"Subject with ID {requestModel.SubjectId} does not exist.");
        }

        test.SubjectId = requestModel.SubjectId;
        test.Name = requestModel.Name;
        test.Questions = requestModel.Questions.Select(question => new Question
        {
            Text = question.Text,
            Options = question.Options.Select(questionOption => new QuestionOption
            {
                OptionText = questionOption.OptionText,
                IsCorrect = questionOption.IsCorrect
            }).ToList().AsReadOnly()
        }).ToList().AsReadOnly();

        await testRepository.UpdateTestAsync(test);
        return Ok();
    }
}
