using System.Security.Claims;
using Jahoot.Core.Models;
using Jahoot.WebApi.Models.Responses;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Student;

[Route("api/student/tests/upcoming")]
[ApiController]
[Tags("Student")]
[Authorize(Policy = nameof(Role.Student))]
public class UpcomingTestsController(IStudentRepository studentRepository, ITestRepository testRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UpcomingTestResponse>>> GetUpcomingTests()
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

        IEnumerable<UpcomingTestResponse> upcomingTests = await testRepository.GetUpcomingTestsForStudentAsync(student.StudentId);
        return Ok(upcomingTests);
    }
}
