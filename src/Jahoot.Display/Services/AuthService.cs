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

    public async Task<RegisterResult> Register(StudentRegistrationRequestModel registerRequest)
    {
        var response = await _httpClient.PostAsJsonAsync("api/student/register/new", registerRequest);

        if (response.IsSuccessStatusCode)
        {
            return new RegisterResult { Success = true, ErrorMessage = string.Empty };
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();

            try
            {
                // Try to parse as JSON if the server returns a JSON error model
                using (var jsonDoc = JsonDocument.Parse(errorContent))
                {
                    // Check for standard error message format
                    if (jsonDoc.RootElement.TryGetProperty("message", out var messageElement))
                    {
                        return new RegisterResult { Success = false, ErrorMessage = messageElement.GetString() ?? "An unknown error occurred." };
                    }
                    
                    // Check for validation errors (ModelState) which is usually a dictionary of arrays
                    // Example: { "Email": ["Invalid email"] }
                    // Simple approach: just return the raw content if it's not a simple message, 
                    // or try to extract the first error. For now, let's keep it simple.
                }
            }
            catch (JsonException)
            {
                // Not JSON, return raw string (e.g. plain text "User already exists")
                return new RegisterResult { Success = false, ErrorMessage = errorContent };
            }

            // If we parsed JSON but didn't find "message", it might be a complex validation error object.
            // In a real app we'd parse this better, but for now fallback to the raw content or a generic message.
            // If errorContent is short enough, use it.
            return new RegisterResult { Success = false, ErrorMessage = !string.IsNullOrWhiteSpace(errorContent) ? errorContent : "Registration failed." };
        }
    }
}
