using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jahoot.Display.Services;

public interface ISubjectService
{
    Task<IEnumerable<Subject>> GetAllSubjectsAsync();
    Task<Result> CreateSubjectAsync(CreateSubjectRequestModel request);
    Task<Result> UpdateSubjectAsync(int id, UpdateSubjectRequestModel request);
}
