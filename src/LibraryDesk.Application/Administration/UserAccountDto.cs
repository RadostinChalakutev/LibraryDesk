using LibraryDesk.Domain.Security;

namespace LibraryDesk.Application.Administration;

public sealed record UserAccountDto(
    Guid Id,
    string DisplayName,
    string Username,
    UserRole Role,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastSignedInAtUtc);
