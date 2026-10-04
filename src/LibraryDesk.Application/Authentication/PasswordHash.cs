namespace LibraryDesk.Application.Authentication;

/// <summary>
/// Contains the encoded password hash and salt required for password verification.
/// </summary>
public sealed record PasswordHash(string Hash, string Salt);
