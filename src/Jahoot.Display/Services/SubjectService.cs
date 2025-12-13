using Jahoot.Core.Models;
using Jahoot.Core.Models.Requests;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Jahoot.Display.Services;

public class SubjectService(HttpClient httpClient, ISecureStorageService secureStorageService) : ISubjectService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private void AddAuthorizationHeader()
    {
        var token = secureStorageService.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<IEnumerable<Subject>> GetAllSubjectsAsync()
    {
        AddAuthorizationHeader();
        var response = await httpClient.GetAsync("api/subject/list");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<Subject>>(_jsonOptions) ?? [];
        }
        return [];
    }

    public async Task<Result> CreateSubjectAsync(CreateSubjectRequestModel request)
    {
        AddAuthorizationHeader();
        var response = await httpClient.PostAsJsonAsync("api/subject", request);
        if (response.IsSuccessStatusCode)
        {
            return new Result { Success = true };
        }
        
        var error = await response.Content.ReadAsStringAsync();
        return new Result { Success = false, ErrorMessage = error };
    }

    public async Task<Result> UpdateSubjectAsync(int id, UpdateSubjectRequestModel request)
    {
        AddAuthorizationHeader();
        var response = await httpClient.PutAsJsonAsync($"api/subject/{id}", request);
        if (response.IsSuccessStatusCode)
        {
            return new Result { Success = true };
        }

        var error = await response.Content.ReadAsStringAsync();
        return new Result { Success = false, ErrorMessage = error };
    }
}
