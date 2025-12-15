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

    private HttpClient CreateClient()
    {
        var token = secureStorageService.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return httpClient;
    }

    public async Task<IEnumerable<Subject>> GetAllSubjectsAsync()
    {
        var client = CreateClient();
        var response = await client.GetAsync("api/subject/list");
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<Subject>>(_jsonOptions) ?? [];
        }

        // Throw exception to be handled by the caller
        throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
    }

    public async Task<Result> CreateSubjectAsync(CreateSubjectRequestModel request)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("api/subject", request);
        
        if (response.IsSuccessStatusCode)
        {
            return new Result { Success = true };
        }
        
        var error = await response.Content.ReadAsStringAsync();
        return new Result { Success = false, ErrorMessage = $"Error {(int)response.StatusCode}: {error}" };
    }

    public async Task<Result> UpdateSubjectAsync(int id, UpdateSubjectRequestModel request)
    {
        var client = CreateClient();
        var response = await client.PutAsJsonAsync($"api/subject/{id}", request);
        
        if (response.IsSuccessStatusCode)
        {
            return new Result { Success = true };
        }

        var error = await response.Content.ReadAsStringAsync();
        return new Result { Success = false, ErrorMessage = $"Error {(int)response.StatusCode}: {error}" };
    }
}
