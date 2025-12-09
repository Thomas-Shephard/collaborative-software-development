using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Subject;

[Route("api/subject")]
[ApiController]
public class CreateSubjectController(ISubjectRepository subjectRepository) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = nameof(Role.Admin))]
    public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectRequestModel requestModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Core.Models.Subject? existingSubject = await subjectRepository.GetSubjectByNameAsync(requestModel.Name);
        if (existingSubject is not null)
        {
            return Conflict("Subject with this name already exists.");
        }

        await subjectRepository.CreateSubjectAsync(requestModel.Name);
        return StatusCode(201);
    }
}
