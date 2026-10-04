namespace LibraryDesk.Domain.Readers;

/// <summary>
/// A registered library reader who may borrow physical copies.
/// </summary>
public sealed class Reader
{
    private Reader()
    {
        MembershipNumber = string.Empty;
        FullName = string.Empty;
    }

    private Reader(
        Guid id,
        string membershipNumber,
        string fullName,
        string? email,
        string? phone,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        MembershipNumber = membershipNumber;
        FullName = fullName;
        Email = email;
        Phone = phone;
        CreatedAtUtc = createdAtUtc;
        IsActive = true;
    }

    public Guid Id { get; private set; }

    public string MembershipNumber { get; private set; }

    public string FullName { get; private set; }

    public string? Email { get; private set; }

    public string? Phone { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static Reader Create(
        string membershipNumber,
        string fullName,
        string? email,
        string? phone,
        DateTimeOffset createdAtUtc) =>
        new(
            Guid.NewGuid(),
            NormalizeMembershipNumber(membershipNumber),
            NormalizeRequired(fullName, "Reader name"),
            NormalizeOptional(email),
            NormalizeOptional(phone),
            createdAtUtc);

    public void Update(string fullName, string? email, string? phone)
    {
        FullName = NormalizeRequired(fullName, "Reader name");
        Email = NormalizeOptional(email);
        Phone = NormalizeOptional(phone);
    }

    public void Deactivate() => IsActive = false;

    public void Reactivate() => IsActive = true;

    private static string NormalizeMembershipNumber(string membershipNumber)
    {
        var normalized = NormalizeRequired(membershipNumber, "Membership number").ToUpperInvariant();
        if (normalized.Length > 32)
        {
            throw new ArgumentException("Membership number must not exceed 32 characters.", nameof(membershipNumber));
        }

        return normalized;
    }

    private static string NormalizeRequired(string value, string label)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{label} is required.", nameof(value));
        }

        var normalized = value.Trim();
        if (normalized.Length > 160)
        {
            throw new ArgumentException($"{label} must not exceed 160 characters.", nameof(value));
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
