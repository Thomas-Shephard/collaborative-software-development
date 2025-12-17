using Jahoot.Core.Models;
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
    public async Task<IActionResult> GetStudents([FromQuery] bool isApproved)
    {
        IEnumerable<Core.Models.Student> students = await studentRepository.GetStudentsByApprovalStatusAsync(isApproved);
        return Ok(students);
    }
}
