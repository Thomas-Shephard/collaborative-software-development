using Jahoot.Core.Models;

namespace Jahoot.WebApi.Repositories;

public interface ISubjectRepository
{
    Task CreateSubjectAsync(string name);
    Task<IEnumerable<Subject>> GetAllSubjectsAsync();
    Task<Subject?> GetSubjectByIdAsync(int id);
    Task<Subject?> GetSubjectByNameAsync(string name);
    Task UpdateSubjectAsync(Subject subject);
}
