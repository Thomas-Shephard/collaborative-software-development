using Jahoot.Core.Models;
using Jahoot.Core.Utils;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Student;

[Route("api/student/register")]
[ApiController]
public class RegisterController(IStudentRepository studentRepository, IUserRepository userRepository) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RegisterStudent([FromBody] StudentRegistrationRequestModel requestModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        User? existingUser = await userRepository.GetUserByEmailAsync(requestModel.Email);
        if (existingUser is not null && existingUser.Roles.Contains(Role.Student))
        {
            return Conflict("A user with this email address is already a student.");
        }

        string hashedPassword = PasswordUtils.HashPasswordWithSalt(requestModel.Password);

        await studentRepository.CreateStudentAsync(requestModel.Name, requestModel.Email, hashedPassword);

        return StatusCode(201);
    }
}
