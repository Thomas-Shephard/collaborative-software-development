using Jahoot.Core.Models;
using Jahoot.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jahoot.WebApi.Controllers.Test;

[Route("api/test/{testId:int}")]
[ApiController]
[Tags("Test")]
public class DeleteTestController(ITestRepository testRepository) : ControllerBase
{
    [HttpDelete]
    [Authorize(Policy = nameof(Role.Lecturer))]
    public async Task<IActionResult> DeleteTest(int testId)
    {
        Core.Models.Test? existingTest = await testRepository.GetTestByIdAsync(testId);
        if (existingTest is null)
        {
            return NotFound();
        }

        await testRepository.DeleteTestAsync(testId);
        return Ok();
    }
}
