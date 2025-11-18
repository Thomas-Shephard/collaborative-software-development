namespace Jahoot.Display.Services;

/// <summary>
/// Implement secure storage service - DPAPI.
/// </summary>
public interface ISecureStorageService
{
    /// <summary>
    /// Puts a token into safe storage.
    /// </summary>
    /// <param name="token">The token we want to hide.</param>
    void SaveToken(string token);

    /// <summary>
    /// Gets the secret token back from storage.
    /// </summary>
    /// <returns>The token, or nothing if it's not there.</returns>
    string? GetToken();

    /// <summary>
    /// Deletes the token from safe storage.
    /// </summary>
    void DeleteToken();
}
