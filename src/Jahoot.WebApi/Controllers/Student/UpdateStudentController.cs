using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Student;

[Route("api/student/{userId:int}")]
[ApiController]
[Tags("Student")]
public class UpdateStudentController(IStudentRepository studentRepository, IUserRepository userRepository) : ControllerBase
{
    [HttpPut]
    [Authorize(Policy = nameof(Role.Lecturer))]
    public async Task<IActionResult> UpdateStudent(int userId, [FromBody] UpdateStudentRequestModel requestModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Core.Models.Student? student = await studentRepository.GetStudentByUserIdAsync(userId);
        if (student is null)
        {
            return NotFound($"Student with user ID {userId} not found.");
        }

        User? existingUser = await userRepository.GetUserByEmailAsync(requestModel.Email);
        if (existingUser is not null && existingUser.UserId != userId)
        {
            return Conflict("A user with this email address already exists.");
        }

        student.AccountStatus = requestModel.AccountStatus;
        student.Name = requestModel.Name;
        student.Email = requestModel.Email;

        await userRepository.UpdateUserAsync(student);
        await studentRepository.UpdateStudentAsync(student);

        return Ok();
    }
}
