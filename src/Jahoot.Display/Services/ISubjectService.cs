using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Display.Services;

public interface ISubjectService
{
    Task<IEnumerable<Subject>> GetAllSubjectsAsync();
    Task<Result> CreateSubjectAsync(CreateSubjectRequestModel request);
    Task<Result> UpdateSubjectAsync(int id, UpdateSubjectRequestModel request);
}
