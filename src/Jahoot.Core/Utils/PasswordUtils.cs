using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Jahoot.Core.Utils;

// a helper for all our password stuff
public static class PasswordUtils
{
    // we should probably follow what NIST says:
    // salt should be at least 128 bits (16 bytes)
    // hash function should be SHA-256
    
    // OWASP says we should do at least 600,000 iterations
    
    private const int SaltLength = 16;
    private const int HashLength = 32;
    private const KeyDerivationPrf Prf = KeyDerivationPrf.HMACSHA256;
    private const int IterationCount = 600_000;

    // this is a dummy password hash to stop people from guessing users by how long it takes to check the password
    private const string DummyPasswordSaltAndHash = "jk7PLv+C/Vzwxos1JITzLCvfdBi2E2NtplmwXvg15UhSHkPN/Iopn71HvJ88aM4I";

    // this takes a password and hashes it with a new salt
    public static string HashPasswordWithSalt(string password)
    {
        byte[] hash = HashPassword(password, out byte[] salt);

        byte[] saltAndHash = new byte[SaltLength + hash.Length];
        Buffer.BlockCopy(salt, 0, saltAndHash, 0, SaltLength);
        Buffer.BlockCopy(hash, 0, saltAndHash, SaltLength, hash.Length);

        return Convert.ToBase64String(saltAndHash);
    }

    // this checks if a password is correct
    public static bool VerifyPassword(string password, string? saltAndHash)
    {
        bool saltAndHashProvided = saltAndHash is not null;
        if (!saltAndHashProvided)
        {
            saltAndHash = DummyPasswordSaltAndHash;
        }

        byte[] convertedSaltAndHash;
        try
        {
            convertedSaltAndHash = Convert.FromBase64String(saltAndHash!);
        }
        catch (FormatException baseException)
        {
            throw new ArgumentException("Salt and Hash lengths are invalid", baseException);
        }

        // make sure the salt and hash are the right length
        if (convertedSaltAndHash.Length != SaltLength + HashLength)
        {
            throw new ArgumentException("Salt and Hash lengths are invalid");
        }

        byte[] salt = new byte[SaltLength];
        Array.Copy(convertedSaltAndHash, 0, salt, 0, SaltLength);

        byte[] hash = new byte[HashLength];
        Array.Copy(convertedSaltAndHash, SaltLength, hash, 0, HashLength);

        byte[] hashedPassword = HashPassword(password, salt);

        // this is a special way to check the hashes that takes the same amount of time, so people can't guess the hash
        return CryptographicOperations.FixedTimeEquals(hashedPassword, hash) & saltAndHashProvided;
    }

    // this hashes a password with a new salt
    private static byte[] HashPassword(string password, out byte[] salt)
    {
        // the salt is just some random bytes to make the hash unique
        salt = RandomNumberGenerator.GetBytes(SaltLength);
        return HashPassword(password, salt);
    }

    // this hashes a password with a given salt
    private static byte[] HashPassword(string password, byte[] salt)
    {
        // Pbkdf2 is the recommended way to hash passwords in .NET
        return KeyDerivation.Pbkdf2(password, salt, Prf, IterationCount, HashLength);
    }
}
