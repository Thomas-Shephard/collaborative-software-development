using Jahoot.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jahoot.Display.Services
{
    public interface ITestService
    {
        Task<IEnumerable<Test>> GetTests(int? subjectId = null);
        Task<Result> CreateTest(Test test);
        Task<Result> UpdateTest(int testId, Test test);
        Task<Result> DeleteTest(int testId);
        Task<bool> HasAttempts(int testId);
    }
}
