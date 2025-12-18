using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;

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

    public async Task<Result> CreateSubjectAsync(CreateSubjectRequestModel request)
    {
        return await httpService.PostAsync("api/subject", request);
    }

    public async Task<Result> UpdateSubjectAsync(int id, UpdateSubjectRequestModel request)
    {
        return await httpService.PutAsync($"api/subject/{id}", request);
    }
}
