using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Display.Services;

public class SubjectService(IHttpService httpService) : ISubjectService
{
    public async Task<IEnumerable<Subject>> GetAllSubjectsAsync()
    {
        return await httpService.GetAsync<IEnumerable<Subject>>("api/subject/list") ?? [];
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
