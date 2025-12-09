using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Subject;

[Route("api/subject/{id:int}")]
[ApiController]
[Tags("Subject")]
public class UpdateSubjectController(ISubjectRepository subjectRepository) : ControllerBase
{
    [HttpPut]
    [Authorize(Policy = nameof(Role.Admin))]
    public async Task<IActionResult> UpdateSubject(int id, [FromBody] UpdateSubjectRequestModel requestModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Core.Models.Subject? subject = await subjectRepository.GetSubjectByIdAsync(id);
        if (subject is null)
        {
            return NotFound();
        }

        Core.Models.Subject? duplicateSubject = await subjectRepository.GetSubjectByNameAsync(requestModel.Name);
        if (duplicateSubject is not null && duplicateSubject.SubjectId != id)
        {
            return Conflict("Subject with this name already exists.");
        }

        subject.Name = requestModel.Name;
        subject.IsActive = requestModel.IsActive;

        await subjectRepository.UpdateSubjectAsync(subject);
        return Ok();
    }
}
