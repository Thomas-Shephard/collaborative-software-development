using System.Security.Claims;
using Jahoot.Core.Models;
using Jahoot.WebApi.Models.Responses;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Student;

[Route("api/student/statistics")]
[ApiController]
[Tags("Student")]
public class StudentStatisticsController(IStudentRepository studentRepository, ITestRepository testRepository) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = nameof(Role.Student))]
    public async Task<ActionResult<StudentStatisticsResponse>> GetStatistics()
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

        StudentStatisticsResponse stats = await testRepository.GetStudentStatisticsAsync(student.StudentId);
        return Ok(stats);
    }
}
