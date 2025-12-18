using Jahoot.Core.Models;
using Jahoot.WebApi.Models.Responses;

namespace Jahoot.WebApi.Repositories;

public interface ITestRepository
{
    Task CreateTestAsync(Test test);
    Task<IEnumerable<Test>> GetAllTestsAsync(int? subjectId = null);
    Task<Test?> GetTestByIdAsync(int testId);
    Task UpdateTestAsync(Test test);
    Task DeleteTestAsync(int testId);
    Task<bool> HasAttemptsAsync(int testId);
    Task<IEnumerable<UpcomingTestResponse>> GetUpcomingTestsForStudentAsync(int studentId);
    Task<IEnumerable<CompletedTestResponse>> GetCompletedTestsForStudentAsync(int studentId);
    Task<StudentStatisticsResponse> GetStudentStatisticsAsync(int studentId);
}
