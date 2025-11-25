using Jahoot.Core.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System;
using System.Threading.Tasks;

namespace Jahoot.Display.Services;
/// <summary>
/// This service helps users log in and out, and keeps their login tokens safe.
/// </summary>
public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient; 
    private readonly ISecureStorageService _secureStorageService; // keep tokens secret
    private readonly ILogger<AuthService> _logger;

    /// <summary>
    /// Sets up our authentication service with all the tools it needs.
    /// </summary>
    /// <param name="httpClient">The tool for talking to the web server.</param>
    /// <param name="secureStorageService">The tool for keeping tokens safe.</param>
    /// <param name="logger">The tool for writing debug messages.</param>
    public AuthService(HttpClient httpClient, ISecureStorageService secureStorageService, ILogger<AuthService> logger)
    {
        _httpClient = httpClient;
        _secureStorageService = secureStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Tries to log a user in. It sends their details to the web server.
    /// If successful, it saves the special login token.
    /// </summary>
    /// <param name="loginRequest">The user's email and password.</param>
    /// <returns>
    /// True if login worked, false if it didn't, and contains a message
    /// </returns>
    public async Task<Tuple<bool, string>> Login(LoginRequest loginRequest)
    {
        // Send the login info to the server
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);

        if (response.IsSuccessStatusCode)
        {
            // Get the secret token from the server
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
            _logger.LogError("Login failed with status code {StatusCode} and content: {ErrorContent}", response.StatusCode, errorContent);

            // Extract the error message from the server's response
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
                // Show full error content if it's not JSON
                return new Tuple<bool, string>(false, errorContent);
            }

            // If all else fails
            return new Tuple<bool, string>(false, "An unknown error occurred.");
        }
    }

    /// <summary>
    /// Makes the user log out by deleting their secret token.
    /// </summary>
    public void Logout()
    {
        _secureStorageService.DeleteToken();
    }
}
