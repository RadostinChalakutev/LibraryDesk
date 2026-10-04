namespace LibraryDesk.Infrastructure.Persistence.Models;

/// <summary>
/// Stores small pieces of system-owned metadata required to maintain a local installation.
/// </summary>
internal sealed class ApplicationMetadata
{
    public ApplicationMetadata(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public string Key { get; private set; }

    public string Value { get; private set; }
}
