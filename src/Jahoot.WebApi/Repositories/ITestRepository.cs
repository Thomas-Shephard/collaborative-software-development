using Jahoot.Core.Models;

namespace Jahoot.WebApi.Repositories;

public interface ITestRepository
{
    Task CreateTestAsync(Test test);
    Task<IEnumerable<Test>> GetAllTestsAsync(int? subjectId = null);
    Task<Test?> GetTestByIdAsync(int testId);
    Task UpdateTestAsync(Test test);
    Task DeleteTestAsync(int testId);
    Task<bool> HasAttemptsAsync(int testId);
}
