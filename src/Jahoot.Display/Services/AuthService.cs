using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System;
using System.Threading.Tasks;
using Jahoot.Core.Models.Requests;

namespace Jahoot.Display.Services;
public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ISecureStorageService _secureStorageService;

    public AuthService(HttpClient httpClient, ISecureStorageService secureStorageService)
    {
        _httpClient = httpClient;
        _secureStorageService = secureStorageService;
    }

    public async Task<LoginResult> Login(LoginRequestModel loginRequest)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            using (var jsonDoc = JsonDocument.Parse(jsonResponse))
            {
                if (jsonDoc.RootElement.TryGetProperty("token", out var tokenElement))
                {
                    var token = tokenElement.GetString();
                    if (token != null)
                    {
                        _secureStorageService.SaveToken(token);
                        return new LoginResult { Success = true, ErrorMessage = string.Empty };
                    }
                }
            }
            return new LoginResult { Success = false, ErrorMessage = "Token not found in response." };
        }
        else
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return new LoginResult { Success = false, ErrorMessage = "Your credentials were incorrect." };
            }

            var errorContent = await response.Content.ReadAsStringAsync();

            try
            {
                using (var jsonDoc = JsonDocument.Parse(errorContent))
                {
                    if (jsonDoc.RootElement.TryGetProperty("message", out var messageElement))
                    {
                        return new LoginResult { Success = false, ErrorMessage = messageElement.GetString() ?? "An unknown error occurred." };
                    }
                }
            }
            catch (JsonException)
            {
                return new LoginResult { Success = false, ErrorMessage = errorContent };
            }

            return new LoginResult { Success = false, ErrorMessage = "An unknown error occurred." };
        }
    }

    public async Task Logout()
    {
        _secureStorageService.DeleteToken();
        await _httpClient.PostAsync("api/auth/logout", null);
    }
}
