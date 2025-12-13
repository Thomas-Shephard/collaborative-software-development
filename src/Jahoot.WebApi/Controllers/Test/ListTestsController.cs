using Jahoot.Core.Models;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Test;

[Route("api/test")]
[ApiController]
[Tags("Test")]
public class ListTestsController(ITestRepository testRepository) : ControllerBase
{
    [HttpGet("list")]
    [Authorize(Policy = nameof(Role.Lecturer))]
    public async Task<IActionResult> ListTests([FromQuery] int? subjectId)
    {
        IEnumerable<Core.Models.Test> tests = await testRepository.GetAllTestsAsync(subjectId);
        return Ok(tests);
    }
}
