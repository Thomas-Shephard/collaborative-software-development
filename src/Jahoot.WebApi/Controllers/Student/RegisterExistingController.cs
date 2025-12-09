using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.Core.Utils;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Student;

[Route("api/student/register/existing")]
[ApiController]
[Tags("Student")]
public class RegisterExistingController(IStudentRepository studentRepository, IUserRepository userRepository) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RegisterExistingStudent([FromBody] ExistingUserRegistrationRequestModel requestModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        User? user = await userRepository.GetUserByEmailAsync(requestModel.Email);
        if (user is null || !PasswordUtils.VerifyPassword(requestModel.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid email or password.");
        }

        if (user.Roles.Contains(Role.Student))
        {
            return Conflict("User is already a student.");
        }

        await studentRepository.CreateStudentAsync(user.UserId);

        return StatusCode(201);
    }
}
