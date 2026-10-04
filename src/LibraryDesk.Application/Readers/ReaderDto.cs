namespace LibraryDesk.Application.Readers;

/// <summary>
/// Reader information used by application workflows and the desktop UI.
/// </summary>
public sealed record ReaderDto(
    Guid Id,
    string MembershipNumber,
    string FullName,
    string? Email,
    string? Phone,
    bool IsActive,
    DateTimeOffset CreatedAtUtc);
