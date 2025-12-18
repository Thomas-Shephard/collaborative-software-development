using System.Security.Claims;
using Jahoot.Core.Models;
using Jahoot.WebApi.Models.Responses;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Student;

[Route("api/student/tests/completed")]
[ApiController]
[Tags("Student")]
[Authorize(Policy = nameof(Role.Student))]
public class CompletedTestsController(IStudentRepository studentRepository, ITestRepository testRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompletedTestResponse>>> GetCompletedTests()
    {
        string? userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out int userId))
        {
            return BadRequest();
        }

        Core.Models.Student? student = await studentRepository.GetStudentByUserIdAsync(userId);
        if (student is null)
        {
            return NotFound();
        }

        IEnumerable<CompletedTestResponse> completedTests = await testRepository.GetCompletedTestsForStudentAsync(student.StudentId);
        return Ok(completedTests);
    }
}
