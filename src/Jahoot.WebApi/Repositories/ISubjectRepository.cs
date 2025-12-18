using Jahoot.Core.Models;
using Jahoot.WebApi.Models.Responses;

namespace Jahoot.WebApi.Repositories;

public interface ISubjectRepository
{
    Task CreateSubjectAsync(string name);
    Task<IEnumerable<Subject>> GetAllSubjectsAsync(bool? isActive = null);
    Task<Subject?> GetSubjectByIdAsync(int id);
    Task<IEnumerable<Subject>> GetSubjectsByIdsAsync(IEnumerable<int> ids);
    Task<Subject?> GetSubjectByNameAsync(string name);
    Task<IEnumerable<LeaderboardEntry>> GetLeaderboardForSubjectAsync(int subjectId);
    Task UpdateSubjectAsync(Subject subject);
}
