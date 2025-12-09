using Jahoot.Core.Models;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Subject;

[Route("api/subject/list")]
[ApiController]
public class ListSubjectsController(ISubjectRepository subjectRepository) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ListSubjects()
    {
        IEnumerable<Core.Models.Subject> subjects = await subjectRepository.GetAllSubjectsAsync();
        return Ok(subjects);
    }
}
