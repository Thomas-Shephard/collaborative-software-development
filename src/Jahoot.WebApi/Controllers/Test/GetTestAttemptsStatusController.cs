using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Jahoot.Core.Models;

namespace Jahoot.WebApi.Controllers.Test;

[Route("api/test/{testId:int}/has-attempts")]
[ApiController]
[Tags("Test")]
public class GetTestAttemptsStatusController(ITestRepository testRepository) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = nameof(Role.Lecturer))]
    public async Task<IActionResult> HasAttempts(int testId)
    {
        Core.Models.Test? test = await testRepository.GetTestByIdAsync(testId);
        if (test is null)
        {
            return NotFound();
        }

        bool hasAttempts = await testRepository.HasAttemptsAsync(testId);
        return Ok(new { HasAttempts = hasAttempts });
    }
}
