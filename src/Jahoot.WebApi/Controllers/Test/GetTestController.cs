using Jahoot.Core.Models;
using Jahoot.WebApi.Models.Responses.Test;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Test;

[Route("api/test/{testId:int}")]
[ApiController]
[Tags("Test")]
public class GetTestController(ITestRepository testRepository, ISubjectRepository subjectRepository) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = nameof(Role.Student))]
    public async Task<ActionResult<StudentTestDetailResponse>> GetTest(int testId)
    {
        Core.Models.Test? test = await testRepository.GetTestByIdAsync(testId);
        if (test is null)
        {
            return NotFound();
        }

        Core.Models.Subject? subject = await subjectRepository.GetSubjectByIdAsync(test.SubjectId);
        if (subject is null)
        {
            return StatusCode(500, "Test has invalid subject");
        }

        if (!subject.IsActive)
        {
            return StatusCode(403, "The subject is disabled.");
        }

        Random random = new();
        List<StudentQuestionResponse> questions = test.Questions
                                                      .OrderBy(_ => random.Next())
                                                      .Take(test.NumberOfQuestions)
                                                      .Select(q => new StudentQuestionResponse
                                                      {
                                                          QuestionId = q.QuestionId,
                                                          Text = q.Text,
                                                          Options = q.Options
                                                                     .Select(o => new StudentQuestionOptionResponse
                                                                     {
                                                                         QuestionOptionId = o.QuestionOptionId,
                                                                         OptionText = o.OptionText
                                                                     })
                                                                     .OrderBy(_ => random.Next())
                                                                     .ToList()
                                                      })
                                                      .ToList();

        return Ok(new StudentTestDetailResponse
        {
            TestId = test.TestId,
            Name = test.Name,
            SubjectName = subject.Name,
            NumberOfQuestions = test.NumberOfQuestions,
            Questions = questions
        });
    }
}
