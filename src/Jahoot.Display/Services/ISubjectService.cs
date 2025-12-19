using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jahoot.Display.Services;

public interface ISubjectService
{
    Task<IEnumerable<Subject>> GetAllSubjectsAsync(bool? isActive = null);
    Task<Result> CreateSubjectAsync(CreateSubjectRequestModel request);
    Task<Result> UpdateSubjectAsync(int id, UpdateSubjectRequestModel request);
    Task<IEnumerable<Subject>> GetSubjects();
    Task<IEnumerable<LeaderboardEntry>> GetLeaderboardForSubject(int subjectId);
}
