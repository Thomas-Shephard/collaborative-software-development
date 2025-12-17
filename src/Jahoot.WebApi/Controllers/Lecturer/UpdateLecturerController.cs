using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Lecturer;

[Route("api/lecturer/{userId:int}")]
[ApiController]
[Tags("Lecturer")]
public class UpdateLecturerController(ILecturerRepository lecturerRepository, IUserRepository userRepository) : ControllerBase
{
    [HttpPut]
    [Authorize(Policy = nameof(Role.Admin))]
    public async Task<IActionResult> UpdateLecturer(int userId, [FromBody] UpdateLecturerRequestModel request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Jahoot.Core.Models.Lecturer? lecturer = await lecturerRepository.GetLecturerByUserIdAsync(userId);
        if (lecturer is null)
        {
            return NotFound($"Lecturer with user ID {userId} not found.");
        }

        User? existingUser = await userRepository.GetUserByEmailAsync(request.Email);
        if (existingUser is not null && existingUser.UserId != userId)
        {
            return Conflict("A user with this email address already exists.");
        }

        lecturer.Name = request.Name;
        lecturer.Email = request.Email;
        lecturer.IsAdmin = request.IsAdmin;
        lecturer.IsDisabled = request.IsDisabled;

        await userRepository.UpdateUserAsync(lecturer);
        await lecturerRepository.UpdateLecturerAsync(lecturer);

        return Ok();
    }
}
