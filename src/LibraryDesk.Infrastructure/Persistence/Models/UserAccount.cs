using LibraryDesk.Domain.Security;

namespace LibraryDesk.Infrastructure.Persistence.Models;

/// <summary>
/// Stores a local user account and only password-verification material, never plaintext credentials.
/// </summary>
internal sealed class UserAccount
{
    public UserAccount(
        Guid id,
        string displayName,
        string username,
        string normalizedUsername,
        string passwordHash,
        string passwordSalt,
        UserRole role,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        DisplayName = displayName;
        Username = username;
        NormalizedUsername = normalizedUsername;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        Role = role;
        CreatedAtUtc = createdAtUtc;
        IsActive = true;
    }

    public Guid Id { get; private set; }

    public string DisplayName { get; private set; }

    public string Username { get; private set; }

    public string NormalizedUsername { get; private set; }

    public string PasswordHash { get; private set; }

    public string PasswordSalt { get; private set; }

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? LastSignedInAtUtc { get; private set; }

    public void RecordSuccessfulSignIn(DateTimeOffset signedInAtUtc) => LastSignedInAtUtc = signedInAtUtc;

    public void SetActive(bool isActive) => IsActive = isActive;
}
