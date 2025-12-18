using System.Security.Claims;
using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Models.Responses;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Test;

[Route("api/test/{testId:int}/submit")]
[ApiController]
[Tags("Test")]
public class SubmitTestController(ITestRepository testRepository, ISubjectRepository subjectRepository, IStudentRepository studentRepository) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = nameof(Role.Student))]
    public async Task<ActionResult<CompletedTestResponse>> SubmitTest(int testId, [FromBody] SubmitTestRequestModel request)
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

        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
        {
            return BadRequest();
        }

        Core.Models.Student? student = await studentRepository.GetStudentByUserIdAsync(userId);
        if (student is null)
        {
            return NotFound("Student not found.");
        }

        if (await testRepository.HasStudentCompletedTestAsync(student.StudentId, testId))
        {
            return BadRequest("You have already completed this test.");
        }

        if (!await studentRepository.IsUserEnrolledInSubjectAsync(userId, test.SubjectId))
        {
            return StatusCode(403, "You are not enrolled in this subject.");
        }

        if (!subject.IsActive)
        {
            return StatusCode(403, "The subject is disabled.");
        }

        int score = 0;
        int questionsCorrect = 0;

        foreach (AnswerRequestModel answer in request.Answers)
        {
            Question? question = test.Questions.FirstOrDefault(q => q.QuestionId == answer.QuestionId);
            if (question is null)
            {
                continue;
            }

            QuestionOption? selectedOption = question.Options.FirstOrDefault(o => o.QuestionOptionId == answer.SelectedOptionId);
            if (selectedOption is { IsCorrect: true })
            {
                score += 30;
                questionsCorrect++;
            }
            else
            {
                score -= 10;
            }
        }

        await testRepository.SaveTestResultAsync(student.StudentId, testId, score, questionsCorrect);

        return Ok(new CompletedTestResponse
        {
            TestId = test.TestId,
            TestName = test.Name,
            SubjectName = subject.Name,
            StudentName = student.Name,
            CompletedDate = DateTime.Now,
            ScorePercentage = (double)questionsCorrect * 100 / test.NumberOfQuestions,
            TotalPoints = score
        });
    }
}
