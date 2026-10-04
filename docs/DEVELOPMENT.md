# Development

## Commands

Run these commands from the repository root:

```powershell
dotnet restore LibraryDesk.sln
dotnet build LibraryDesk.sln --configuration Debug
dotnet test LibraryDesk.sln --configuration Debug --no-build
dotnet run --project src/LibraryDesk.Desktop/LibraryDesk.Desktop.csproj
```

For a Windows x64 release build:

```powershell
dotnet publish src/LibraryDesk.Desktop/LibraryDesk.Desktop.csproj --configuration Release --runtime win-x64 --self-contained false --output release
```

## Conventions

- Treat warnings as errors. Keep nullable annotations correct.
- Keep business rules in Domain or Application, never in Avalonia views or code-behind.
- Register cross-layer implementations through each layer's `Add…` extension method. The Desktop project remains the only composition root.
- Keep database and filesystem access in Infrastructure. Apply all schema changes through EF Core migrations; do not call `EnsureCreated` in application code.
- Add tests in the test project corresponding to the layer under test. Tests must not depend on the Avalonia desktop UI unless a later phase requires dedicated UI testing.

## Database migrations

The repository uses the local `dotnet-ef` tool manifest. Restore it before creating or inspecting migrations:

```powershell
dotnet tool restore
dotnet ef migrations add MeaningfulMigrationName --project src/LibraryDesk.Infrastructure --startup-project src/LibraryDesk.Desktop --context LibraryDeskDbContext --output-dir Persistence/Migrations
dotnet ef database update --project src/LibraryDesk.Infrastructure --startup-project src/LibraryDesk.Desktop --context LibraryDeskDbContext
```

The desktop app applies migrations automatically during startup. In normal development, use a separate configured `Database:FilePath` when you need an isolated database.

## Current boundaries

The project deliberately does not yet include credentials, readers, titles, physical copies, borrowing, reporting, backup, or synchronization. The only persistence model is the system-owned `application_metadata` table used to validate the migration pipeline. Feature pages remain transparent placeholders until their assigned phases.
