using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Subject;

[Route("api/subject/list")]
[ApiController]
[Tags("Subject")]
public class ListSubjectsController(ISubjectRepository subjectRepository) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ListSubjects([FromQuery] bool? isActive = null)
    {
        IEnumerable<Core.Models.Subject> subjects = await subjectRepository.GetAllSubjectsAsync(isActive);
        return Ok(subjects);
    }
}
