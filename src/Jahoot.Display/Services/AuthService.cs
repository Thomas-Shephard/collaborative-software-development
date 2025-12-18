using Jahoot.Core.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Collections.Generic;

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

    public async Task<LoginResult> Login(LoginRequest loginRequest)
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
                        
                        // Decode the JWT token to extract roles
                        var roles = ExtractRolesFromToken(token);
                        
                        return new LoginResult 
                        { 
                            Success = true, 
                            ErrorMessage = string.Empty,
                            UserRoles = roles
                        };
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

    private List<Role> ExtractRolesFromToken(string token)
    {
        try
        {
            // JWT token format: header.payload.signature
            var parts = token.Split('.');
            if (parts.Length != 3)
                return new List<Role>();
            
            // Decode the payload (second part)
            var payload = parts[1];
            
            // Add padding if needed for base64 decoding
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }
            
            var payloadBytes = Convert.FromBase64String(payload);
            var payloadJson = Encoding.UTF8.GetString(payloadBytes);
            
            // Parse the JSON payload
            using var jsonDoc = JsonDocument.Parse(payloadJson);
            var roles = new List<Role>();
            
            // JWT role claims use "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" as the key
            if (jsonDoc.RootElement.TryGetProperty("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out var roleElement))
            {
                // Roles can be a single string or an array of strings
                if (roleElement.ValueKind == JsonValueKind.String)
                {
                    if (Enum.TryParse<Role>(roleElement.GetString(), out var role))
                    {
                        roles.Add(role);
                    }
                }
                else if (roleElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var roleItem in roleElement.EnumerateArray())
                    {
                        if (Enum.TryParse<Role>(roleItem.GetString(), out var role))
                        {
                            roles.Add(role);
                        }
                    }
                }
            }
            
            return roles;
        }
        catch
        {
            // If token parsing fails, return empty list
            return new List<Role>();
        }
    }

    public async Task Logout()
    {
        _secureStorageService.DeleteToken();
        await _httpClient.PostAsync("api/auth/logout", null);
    }
}
