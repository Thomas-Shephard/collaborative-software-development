using System.Security.Claims;
using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Models.Responses;
using Jahoot.WebApi.Repositories;
using Jahoot.WebApi.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Jahoot.WebApi.Controllers.Test;

[Route("api/test/{testId:int}/submit")]
[ApiController]
[Tags("Test")]
public class SubmitTestController(ITestRepository testRepository, ISubjectRepository subjectRepository, IStudentRepository studentRepository, IOptions<ScoringSettings> scoringSettings) : ControllerBase
{
    private readonly ScoringSettings _scoringSettings = scoringSettings.Value;

    [HttpPost]
    [Authorize(Policy = nameof(Role.Student))]
    public async Task<ActionResult<CompletedTestResponse>> SubmitTest(int testId, [FromBody] SubmitTestRequestModel request)
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
        HashSet<int> processedQuestionIds = [];

        foreach (AnswerRequestModel answer in request.Answers)
        {
            if (!processedQuestionIds.Add(answer.QuestionId))
            {
                continue;
            }

            Question? question = test.Questions.FirstOrDefault(q => q.QuestionId == answer.QuestionId);
            QuestionOption? selectedOption = question?.Options.FirstOrDefault(o => o.QuestionOptionId == answer.SelectedOptionId);
            if (selectedOption is null)
            {
                continue;
            }

            if (selectedOption.IsCorrect)
            {
                score += _scoringSettings.PointsPerCorrectAnswer;
                questionsCorrect++;
            }
            else
            {
                score += _scoringSettings.PointsPerIncorrectAnswer;
            }
        }

        await testRepository.SaveTestResultAsync(student.StudentId, testId, score, questionsCorrect);

        return Ok(new CompletedTestResponse
        {
            TestId = test.TestId,
            TestName = test.Name,
            SubjectName = subject.Name,
            StudentName = student.Name,
            CompletedDate = DateTime.UtcNow,
            ScorePercentage = test.NumberOfQuestions > 0 ? (double)questionsCorrect * 100 / test.NumberOfQuestions : 0,
            TotalPoints = score
        });
    }
}
