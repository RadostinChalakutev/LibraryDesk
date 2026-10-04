# Development

## Commands

Run commands from the repository root:

```powershell
dotnet restore LibraryDesk.sln
dotnet build LibraryDesk.sln --configuration Debug
dotnet test LibraryDesk.sln --configuration Debug --no-build
dotnet publish src/LibraryDesk.Desktop/LibraryDesk.Desktop.csproj --configuration Release --runtime win-x64 --self-contained true --output release
```

To run against a disposable local database:

```powershell
$env:LIBRARYDESK_Database__FilePath = "$PWD\\local-data\\LibraryDesk.db"
dotnet run --project src/LibraryDesk.Desktop/LibraryDesk.Desktop.csproj --configuration Debug
```

## Conventions

- Treat warnings as errors and preserve nullable correctness.
- Keep domain rules out of Avalonia code-behind and database operations out of view models.
- Add an Application interface/DTO before adding an Infrastructure implementation.
- Register implementations in `AddApplication` or `AddInfrastructure`; the Desktop project is the only composition root.
- Keep the Domain project free of UI, EF Core, filesystem, and Infrastructure dependencies.
- Prefer an integration test against temporary SQLite for persisted workflow behavior. Always clear SQLite pools before deleting a temporary test database.

## Database migrations

The repository has a local `dotnet-ef` tool manifest. Restore it before creating or inspecting migrations:

```powershell
dotnet tool restore
dotnet ef migrations add MeaningfulMigrationName --project src/LibraryDesk.Infrastructure --startup-project src/LibraryDesk.Desktop --context LibraryDeskDbContext --output-dir Persistence/Migrations
dotnet ef database update --project src/LibraryDesk.Infrastructure --startup-project src/LibraryDesk.Desktop --context LibraryDeskDbContext
```

Do not use `EnsureCreated` in application code and do not modify an applied migration. Add a new migration for every persisted schema change. The desktop client runs pending migrations at startup.

## Configuration and files

- `src/LibraryDesk.Desktop/appsettings.json` is copied to the desktop output.
- Environment variables with the `LIBRARYDESK_` prefix override configuration, for example `LIBRARYDESK_Logging__MinimumLevel=Debug`.
- Default application data: `%LocalAppData%\\LibraryDesk\\LibraryDesk.db`.
- Logs: `%LocalAppData%\\LibraryDesk\\logs`.
- User-generated CSV reports: `%UserProfile%\\Documents\\LibraryDesk\\reports`.
- User-generated backups: `%UserProfile%\\Documents\\LibraryDesk\\backups`.

## Quality checks

Before submitting a change, run restore, a Release build, and the full test suite. Review `git diff --check` and use `dotnet list LibraryDesk.sln package --vulnerable --include-transitive` to inspect known package vulnerabilities. The test suite covers domain invariants, session permissions, database initialization, local authentication, reader records, catalogue/copy creation, transactional loans/returns/history, overdue detection, dashboard counts, CSV export, administrator authorization, and SQLite backup readability.
