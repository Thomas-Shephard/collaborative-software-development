using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.Core.Utils;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Lecturer;

[Route("api/lecturer")]
[ApiController]
[Tags("Lecturer")]
public class RegisterNewLecturerController(ILecturerRepository lecturerRepository, IUserRepository userRepository) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = nameof(Role.Admin))]
    public async Task<IActionResult> RegisterLecturer([FromBody] CreateLecturerRequestModel request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (await userRepository.GetUserByEmailAsync(request.Email) is not null)
        {
            return Conflict("A user with this email address already exists.");
        }

        string hashedPassword = PasswordUtils.HashPasswordWithSalt(request.Password);
        await lecturerRepository.CreateLecturerAsync(request.Name, request.Email, hashedPassword, request.IsAdmin);

        return Created();
    }
}
