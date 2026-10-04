using System.Security.Cryptography;
using System.Text;
using LibraryDesk.Application.Authentication;

namespace LibraryDesk.Infrastructure.Authentication;

/// <summary>
/// Password hasher using PBKDF2-HMAC-SHA512 with a per-account random salt.
/// </summary>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 32;
    private const int IterationCount = 210_000;

    public PasswordHash Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            IterationCount,
            HashAlgorithmName.SHA512,
            HashSizeBytes);

        return new PasswordHash(Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public bool Verify(string password, PasswordHash passwordHash)
    {
        if (string.IsNullOrEmpty(password) || passwordHash is null)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(passwordHash.Salt);
            var expectedHash = Convert.FromBase64String(passwordHash.Hash);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                IterationCount,
                HashAlgorithmName.SHA512,
                HashSizeBytes);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (Exception exception) when (exception is FormatException or ArgumentException)
        {
            return false;
        }
    }
}
