using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Jahoot.Display.Services;
public class SecureStorageService : ISecureStorageService
{
    private static readonly byte[] Entropy = { 1, 2, 3, 4, 5, 6, 7, 8 };
    private readonly string _tokenPath;
    
    public SecureStorageService()
    {
        _tokenPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jahoot", "token");
        Directory.CreateDirectory(Path.GetDirectoryName(_tokenPath)!);
    }
    
    public void SaveToken(string token)
    {
        var encryptedData = ProtectedData.Protect(Encoding.UTF8.GetBytes(token), Entropy, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(_tokenPath, encryptedData);
    }
    
    public string? GetToken()
    {
        if (!File.Exists(_tokenPath))
        {
            return null;
        }
        
        var encryptedData = File.ReadAllBytes(_tokenPath);
        var decryptedData = ProtectedData.Unprotect(encryptedData, Entropy, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(decryptedData);
    }
    
    public void DeleteToken()
    {
        if (File.Exists(_tokenPath))
        {
            File.Delete(_tokenPath);
        }
    }
}

