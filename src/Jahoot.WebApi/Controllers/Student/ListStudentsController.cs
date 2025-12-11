using Jahoot.Core.Models;
using Jahoot.WebApi.Models.Responses;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Student;

[Route("api/student/list")]
[ApiController]
[Tags("Student")]
public class ListStudentsController(IStudentRepository studentRepository) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = nameof(Role.Lecturer))]
    public async Task<IActionResult> GetStudents([FromQuery] StudentAccountStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            return BadRequest($"Invalid student account status. Valid values are: {string.Join(", ", Enum.GetNames<StudentAccountStatus>())}");
        }

        IEnumerable<StudentResponseDto> studentDtos = (await studentRepository.GetStudentsByStatusAsync(status))
                                                                              .Select(StudentResponseDto.FromStudent);
        return Ok(studentDtos);
    }
}
