using Jahoot.Display.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jahoot.Display.Services;

public interface ITestService
{
    Task<IEnumerable<UpcomingTestResponse>> GetUpcomingTestsAsync();
    Task<IEnumerable<CompletedTestResponse>> GetCompletedTestsAsync();
    Task<TestDetailsResponse?> GetTestDetailsAsync(int testId);
    Task<TestSubmissionResponse?> SubmitTestAsync(int testId, Dictionary<int, int> answers);
}
