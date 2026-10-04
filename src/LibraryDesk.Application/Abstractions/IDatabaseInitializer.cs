namespace LibraryDesk.Application.Abstractions;

/// <summary>
/// Applies the current database schema and prepares essential persisted application state.
/// </summary>
public interface IDatabaseInitializer
{
    /// <summary>
    /// Initializes the database in an idempotent manner.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
