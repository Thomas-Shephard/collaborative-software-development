using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using Jahoot.WebApi.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jahoot.Display.Services;

public class SubjectService(IHttpService httpService) : ISubjectService
{
    public async Task<IEnumerable<Subject>> GetAllSubjectsAsync(bool? isActive = null)
    {
        string url = "api/subject/list";
        if (isActive.HasValue)
        {
            url += $"?isActive={isActive.Value}";
        }

        return await httpService.GetAsync<IEnumerable<Subject>>(url) ?? [];
    }

    public async Task<IEnumerable<Subject>> GetSubjects()
    {
        return await GetAllSubjectsAsync(true);
    }

    public async Task<IEnumerable<LeaderboardEntry>> GetLeaderboardForSubject(int subjectId)
    {
        return await httpService.GetAsync<IEnumerable<LeaderboardEntry>>($"api/leaderboard/{subjectId}") ?? [];
    }

    public async Task<Result> CreateSubjectAsync(CreateSubjectRequestModel request)
    {
        return await httpService.PostAsync("api/subject", request);
    }

    public async Task<Result> UpdateSubjectAsync(int id, UpdateSubjectRequestModel request)
    {
        return await httpService.PutAsync($"api/subject/{id}", request);
    }
}
