using Jahoot.Core.Models;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Lecturer;

[Route("api/lecturer/{userId:int}")]
[ApiController]
[Tags("Lecturer")]
public class DeleteLecturerController(ILecturerRepository lecturerRepository, IUserRepository userRepository) : ControllerBase
{
    [HttpDelete]
    [Authorize(Policy = nameof(Role.Admin))]
    public async Task<IActionResult> DeleteLecturer(int userId)
    {
        Jahoot.Core.Models.Lecturer? lecturer = await lecturerRepository.GetLecturerByUserIdAsync(userId);
        if (lecturer is null)
        {
            return NotFound($"Lecturer with user ID {userId} not found.");
        }

        await userRepository.DeleteUserAsync(userId);

        return Ok();
    }
}
