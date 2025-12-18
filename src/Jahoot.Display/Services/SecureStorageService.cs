using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Jahoot.Display.Services;
public class SecureStorageService : ISecureStorageService
{
    private readonly string _tokenPath;
    private string? _cachedToken;
    
    public SecureStorageService()
    {
        _tokenPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jahoot", "token");
        Directory.CreateDirectory(Path.GetDirectoryName(_tokenPath)!);
    }
    
    public void SaveToken(string token, bool persist)
    {
        _cachedToken = token;

        if (persist)
        {
            var encryptedData = ProtectedData.Protect(Encoding.UTF8.GetBytes(token), null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(_tokenPath, encryptedData);
        }
        else
        {
            if (File.Exists(_tokenPath))
            {
                File.Delete(_tokenPath);
            }
        }
    }
    
    public string? GetToken()
    {
        if (_cachedToken != null)
        {
            return _cachedToken;
        }

        if (!File.Exists(_tokenPath))
        {
            return null;
        }
        
        try
        {
            var encryptedData = File.ReadAllBytes(_tokenPath);
            var decryptedData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
            _cachedToken = Encoding.UTF8.GetString(decryptedData);
            return _cachedToken;
        }
        catch
        {
            return null;
        }
    }
    
    public void DeleteToken()
    {
        _cachedToken = null;
        if (File.Exists(_tokenPath))
        {
            File.Delete(_tokenPath);
        }
    }
}