using Jahoot.Core.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System;
using System.Threading.Tasks;

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

    public async Task<Tuple<bool, string>> Login(LoginRequest loginRequest)
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
                        return new Tuple<bool, string>(true, string.Empty);
                    }
                }
            }
            return new Tuple<bool, string>(false, "Token not found in response.");
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            
            try
            {
                using (var jsonDoc = JsonDocument.Parse(errorContent))
                {
                    if (jsonDoc.RootElement.TryGetProperty("message", out var messageElement))
                    {
                        return new Tuple<bool, string>(false, messageElement.GetString() ?? "An unknown error occurred.");
                    }
                }
            }
            catch (JsonException)
            {
                return new Tuple<bool, string>(false, errorContent);
            }

            return new Tuple<bool, string>(false, "An unknown error occurred.");
        }
    }

    public void Logout()
    {
        _secureStorageService.DeleteToken();
    }
}
