namespace LibraryDesk.Application.Administration;

public sealed record DatabaseBackup(string FilePath, DateTimeOffset CreatedAtUtc, long SizeInBytes);
