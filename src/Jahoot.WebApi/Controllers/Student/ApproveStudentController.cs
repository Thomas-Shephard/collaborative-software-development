using Jahoot.Core.Models;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Student;

[Route("api/student/{userId:int}/approve")]
[ApiController]
public class ApproveStudentController(IStudentRepository studentRepository) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = nameof(Role.Lecturer))]
    public async Task<IActionResult> ApproveStudentRegistration(int userId)
    {
        Core.Models.Student? student = await studentRepository.GetStudentByUserIdAsync(userId);

        if (student is null)
        {
            return NotFound($"Student with user ID {userId} not found.");
        }

        if (student.AccountStatus != StudentAccountStatus.PendingApproval)
        {
            return BadRequest($"Student with user ID {userId} is not in PendingApproval status.");
        }

        student.AccountStatus = StudentAccountStatus.Active;
        await studentRepository.UpdateStudentAsync(student);

        return Ok();
    }
}
