namespace LibraryDesk.Application.Authentication;

/// <summary>
/// Hashes and verifies passwords without exposing a storage implementation to application services.
/// </summary>
public interface IPasswordHasher
{
    PasswordHash Hash(string password);

    bool Verify(string password, PasswordHash passwordHash);
}
