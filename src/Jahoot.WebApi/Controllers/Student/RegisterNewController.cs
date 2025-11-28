using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.Core.Utils;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Student;

[Route("api/student/register/new")]
[ApiController]
public class RegisterNewController(IStudentRepository studentRepository, IUserRepository userRepository) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RegisterNewStudent([FromBody] StudentRegistrationRequestModel requestModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        User? existingUser = await userRepository.GetUserByEmailAsync(requestModel.Email);
        if (existingUser is not null)
        {
            return Conflict("A user with this email address already exists.");
        }

        string hashedPassword = PasswordUtils.HashPasswordWithSalt(requestModel.Password);

        await studentRepository.CreateStudentAsync(requestModel.Name, requestModel.Email, hashedPassword);

        return StatusCode(201);
    }
}
