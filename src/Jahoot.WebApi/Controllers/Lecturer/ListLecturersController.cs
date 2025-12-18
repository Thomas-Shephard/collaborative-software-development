using Jahoot.Core.Models;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Lecturer;

[Route("api/lecturer/list")]
[ApiController]
[Tags("Lecturer")]
public class ListLecturersController(ILecturerRepository lecturerRepository) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = nameof(Role.Admin))]
    public async Task<IActionResult> GetLecturers()
    {
        IEnumerable<Core.Models.Lecturer> lecturers = await lecturerRepository.GetLecturersAsync();
        return Ok(lecturers);
    }
}
