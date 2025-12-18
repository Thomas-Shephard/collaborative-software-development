using Jahoot.WebApi.Models.Responses;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Leaderboard;

[Route("api/leaderboard/{subjectId:int}")]
[ApiController]
[Tags("Leaderboard")]
public class GetSubjectLeaderboardController(ISubjectRepository subjectRepository) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetLeaderboard(int subjectId)
    {
        Core.Models.Subject? subject = await subjectRepository.GetSubjectByIdAsync(subjectId);
        if (subject is null)
        {
            return BadRequest("No subject with that id exists.");
        }

        if (!subject.IsActive)
        {
            return BadRequest("The subject is disabled.");
        }

        IEnumerable<LeaderboardEntry> leaderboard = await subjectRepository.GetLeaderboardForSubjectAsync(subjectId);
        return Ok(leaderboard);
    }
}
