using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Display.Services;

public class LecturerService(IHttpService httpService) : ILecturerService
{
    public async Task<IEnumerable<Lecturer>> GetAllLecturersAsync()
    {
        return await httpService.GetAsync<IEnumerable<Lecturer>>("api/lecturer/list") ?? [];
    }

    public async Task<Result> CreateLecturerAsync(CreateLecturerRequestModel request)
    {
        return await httpService.PostAsync("api/lecturer", request);
    }

    public async Task<Result> UpdateLecturerAsync(int id, UpdateLecturerRequestModel request)
    {
        return await httpService.PutAsync($"api/lecturer/{id}", request);
    }

    public async Task<Result> DeleteLecturerAsync(int id)
    {
        return await httpService.DeleteAsync($"api/lecturer/{id}");
    }
}
