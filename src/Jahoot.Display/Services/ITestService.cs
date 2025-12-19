using Jahoot.Core.Models;
using Jahoot.Display.Models;
using Jahoot.WebApi.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jahoot.Display.Services
{
    public interface ITestService
    {
        Task<IEnumerable<Test>> GetTests(int? subjectId = null);
        Task<Test?> GetTestById(int testId);
        Task<Result> CreateTest(Test test);
        Task<Result> UpdateTest(int testId, Test test);
        Task<Result> DeleteTest(int testId);
        Task<bool> HasAttempts(int testId);
        Task<IEnumerable<Jahoot.WebApi.Models.Responses.CompletedTestResponse>> GetRecentCompletedTests(int? days = null);
        
        Task<IEnumerable<Jahoot.WebApi.Models.Responses.UpcomingTestResponse>> GetUpcomingTestsAsync();
        Task<IEnumerable<Jahoot.WebApi.Models.Responses.CompletedTestResponse>> GetCompletedTestsAsync();
        Task<TestDetailsResponse?> GetTestDetailsAsync(int testId);
        Task<TestSubmissionResponse?> SubmitTestAsync(int testId, Dictionary<int, int> answers);
    }
}
