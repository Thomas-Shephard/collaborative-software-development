using Jahoot.Core.Models;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Student;

[Route("api/student/{userId:int}")]
[ApiController]
[Tags("Student")]
public class DeleteStudentController(IStudentRepository studentRepository, IUserRepository userRepository) : ControllerBase
{
    [HttpDelete]
    [Authorize(Policy = nameof(Role.Lecturer))]
    public async Task<IActionResult> DeleteStudent(int userId)
    {
        Jahoot.Core.Models.Student? student = await studentRepository.GetStudentByUserIdAsync(userId);
        if (student is null)
        {
            return NotFound($"Student with user ID {userId} not found.");
        }

        await userRepository.DeleteUserAsync(userId);

        return Ok();
    }
}
