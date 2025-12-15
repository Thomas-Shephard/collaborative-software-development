using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Lecturer;

[Route("api/lecturer/assign-role")]
[ApiController]
[Tags("Lecturer")]
public class AssignLecturerRoleController(ILecturerRepository lecturerRepository, IUserRepository userRepository) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = nameof(Role.Admin))]
    public async Task<IActionResult> AssignRoleToExistingUser([FromBody] AssignLecturerRoleRequestModel request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        User? user = await userRepository.GetUserByEmailAsync(request.Email);
        if (user is null)
        {
            return NotFound("User with provided email not found.");
        }

        if (await lecturerRepository.GetLecturerByUserIdAsync(user.UserId) is not null)
        {
            return Conflict("User is already assigned the lecturer role.");
        }

        await lecturerRepository.CreateLecturerAsync(user.UserId, request.IsAdmin);

        return Ok();
    }
}
