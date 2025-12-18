using Jahoot.Core.Models;
using Jahoot.WebApi.Models.Responses;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Lecturer;

[Route("api/lecturer/recent-activity")]
[ApiController]
[Tags("Lecturer")]
public class GetRecentStudentActivityController(ITestRepository testRepository) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = nameof(Role.Lecturer))]
    public async Task<IActionResult> GetRecentActivity()
    {
        IEnumerable<CompletedTestResponse> recentTests = await testRepository.GetRecentCompletedTestsAsync(7);
        return Ok(recentTests);
    }
}
