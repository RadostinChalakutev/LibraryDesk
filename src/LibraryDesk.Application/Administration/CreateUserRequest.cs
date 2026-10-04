using LibraryDesk.Domain.Security;

namespace LibraryDesk.Application.Administration;

/// <summary>
/// Contains the details required for an administrator to provision a local staff account.
/// </summary>
public sealed record CreateUserRequest(string DisplayName, string Username, string Password, UserRole Role);
