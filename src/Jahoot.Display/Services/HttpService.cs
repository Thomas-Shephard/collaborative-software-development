using Jahoot.Core.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Jahoot.Display.Services;

public class HttpService(HttpClient httpClient, ISecureStorageService secureStorageService) : IHttpService
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
        else
        {
            httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<T?> GetAsync<T>(string uri)
    {
        AddAuthorizationHeader();
        var response = await httpClient.GetAsync(uri);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
        }

        throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string uri, TRequest data)
    {
        AddAuthorizationHeader();
        var response = await httpClient.PostAsJsonAsync(uri, data);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
        }

        throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
    }

    public async Task<Result> PostAsync<T>(string uri, T data)
    {
        AddAuthorizationHeader();
        var response = await httpClient.PostAsJsonAsync(uri, data);
        return await HandleResponse(response);
    }

    public async Task<Result> PutAsync<T>(string uri, T data)
    {
        AddAuthorizationHeader();
        var response = await httpClient.PutAsJsonAsync(uri, data);
        return await HandleResponse(response);
    }

    public async Task<Result> DeleteAsync(string uri)
    {
        AddAuthorizationHeader();
        var response = await httpClient.DeleteAsync(uri);
        return await HandleResponse(response);
    }

    private static async Task<Result> HandleResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return new Result { Success = true };
        }

        return await ParseErrorResponse(response);
    }

    private static async Task<Result> ParseErrorResponse(HttpResponseMessage response)
    {
        var errorContent = await response.Content.ReadAsStringAsync();
        try
        {
            using var jsonDoc = JsonDocument.Parse(errorContent);
            if (jsonDoc.RootElement.TryGetProperty("message", out var messageElement))
            {
                return new Result { Success = false, ErrorMessage = messageElement.GetString() };
            }
        }
        catch
        {
            // Ignored, fallback to content
        }

        if (!string.IsNullOrWhiteSpace(errorContent))
        {
            return new Result { Success = false, ErrorMessage = errorContent };
        }

        return new Result { Success = false, ErrorMessage = $"Request failed: {response.StatusCode}" };
    }
}
