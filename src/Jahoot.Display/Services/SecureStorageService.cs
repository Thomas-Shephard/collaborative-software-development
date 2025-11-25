using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Jahoot.Display.Services;
    /// <summary>
    /// Implements DPAPI to securely store and retrieve authentication tokens.
    /// </summary>
    public class SecureStorageService : ISecureStorageService
    {
        private static readonly byte[] Entropy = { 1, 2, 3, 4, 5, 6, 7, 8 };
        private readonly string _tokenPath;

        /// <summary>
        /// Sets up the secure storage. It figures out where to save the token file
        /// and makes sure that folder exists.
        /// </summary>
        public SecureStorageService()
        {
            _tokenPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jahoot", "token");
            Directory.CreateDirectory(Path.GetDirectoryName(_tokenPath)!);
        }

        /// <summary>
        /// Takes a token, encrypts it, and saves it to a file.
        /// </summary>
        /// <param name="token">The token we want to encrypt and save.</param>
        public void SaveToken(string token)
        {
            var encryptedData = ProtectedData.Protect(Encoding.UTF8.GetBytes(token), Entropy, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(_tokenPath, encryptedData);
        }

        /// <summary>
        /// Reads the encrypted token from the file and decrypts it.
        /// </summary>
        /// <returns>The original, unencrypted token, or nothing if the file isn't found.</returns>
        public string? GetToken()
        {
            if (!File.Exists(_tokenPath))
            {
                return null;
            }

            // Encrypt and decrypt the token using DPAPI.
            var encryptedData = File.ReadAllBytes(_tokenPath);
            var decryptedData = ProtectedData.Unprotect(encryptedData, Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decryptedData);
        }

        /// <summary>
        /// Deletes the token file from our secure storage.
        /// </summary>
        public void DeleteToken()
        {
            // Only try to delete if the file is actually there.
            if (File.Exists(_tokenPath))
            {
                File.Delete(_tokenPath);
            }
        }
    }
