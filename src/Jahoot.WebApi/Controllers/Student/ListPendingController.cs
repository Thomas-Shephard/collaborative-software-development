using Jahoot.Core.Models;
using Jahoot.WebApi.Models.Responses;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Student;

[Route("api/student/pending")]
[ApiController]
[Tags("Student")]
public class ListPendingController(IStudentRepository studentRepository) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = nameof(Role.Lecturer))]
    public async Task<IActionResult> GetPendingStudents()
    {
        IEnumerable<StudentResponseDto> studentDtos = (await studentRepository.GetStudentsByStatusAsync(StudentAccountStatus.PendingApproval))
                                                                           .Select(StudentResponseDto.FromStudent);
        return Ok(studentDtos);
    }
}
