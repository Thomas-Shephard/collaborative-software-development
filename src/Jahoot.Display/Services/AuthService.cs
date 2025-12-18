using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Jahoot.Core.Models.Requests;
using Jahoot.Core.Models;
using System.Net;

namespace Jahoot.Display.Services;
public class AuthService(HttpClient httpClient, ISecureStorageService secureStorageService) : IAuthService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ISecureStorageService _secureStorageService = secureStorageService;

    public async Task<Result> Login(LoginRequestModel loginRequest)
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
                        
                        return new Result 
                        { 
                            Success = true, 
                            ErrorMessage = string.Empty,
                            UserRoles = roles
                        };
                    }
                }
            }
            return new Result { Success = false, ErrorMessage = "Token not found in response." };
        }
        else
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadRequest)
            {
                return new Result { Success = false, ErrorMessage = "Your credentials were incorrect." };
            }

            return await ParseErrorResponse(response, "An unknown error occurred.");
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

    public async Task<Result> Register(CreateStudentRequestModel registerRequest)
    {
        var response = await _httpClient.PostAsJsonAsync("api/student", registerRequest);

        if (response.IsSuccessStatusCode)
        {
            return new Result { Success = true, ErrorMessage = string.Empty };
        }
        else
        {
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return new Result { Success = false, ErrorMessage = "The registration details provided are invalid." };
            }

            return await ParseErrorResponse(response, "Registration failed.");
        }
    }

    public async Task<Result> ForgotPassword(string email)
    {
        var model = new ForgotPasswordRequestModel { Email = email };
        var response = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", model);

        if (response.IsSuccessStatusCode)
        {
            return new Result { Success = true, ErrorMessage = string.Empty };
        }
        else
        {
            return await ParseErrorResponse(response, "Failed to request password reset.");
        }
    }

    public async Task<Result> ResetPassword(string email, string token, string newPassword)
    {
        var model = new ResetPasswordRequestModel 
        { 
            Email = email, 
            Token = token, 
            NewPassword = newPassword 
        };
        var response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", model);

        if (response.IsSuccessStatusCode)
        {
            return new Result { Success = true, ErrorMessage = string.Empty };
        }
        else
        {
            return await ParseErrorResponse(response, "Failed to reset password.");
        }
    }

    private static async Task<Result> ParseErrorResponse(HttpResponseMessage response, string defaultMessage)
    {
        var errorContent = await response.Content.ReadAsStringAsync();

        try
        {
            using var jsonDoc = JsonDocument.Parse(errorContent);
            if (jsonDoc.RootElement.TryGetProperty("message", out var messageElement))
            {
                return new Result { Success = false, ErrorMessage = messageElement.GetString() ?? defaultMessage };
            }
        }
        catch (JsonException)
        {
            return new Result { Success = false, ErrorMessage = !string.IsNullOrWhiteSpace(errorContent) ? errorContent : defaultMessage };
        }

        return new Result { Success = false, ErrorMessage = defaultMessage };
    }
}
