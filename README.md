# LibraryDesk

LibraryDesk is a Windows desktop application for library operations. The repository currently contains **Phases 1–2**: a buildable Avalonia shell, clean project boundaries, configuration, dependency injection, logging, and a SQLite/EF Core persistence foundation. Reader, book, copy, and loan workflows have not yet been implemented.

## Prerequisites

- Windows 10 or later (x64)
- [.NET SDK 10.0.400](https://dotnet.microsoft.com/download/dotnet/10.0) or a compatible .NET 10 SDK

## Build and test

```powershell
dotnet restore LibraryDesk.sln
dotnet build LibraryDesk.sln --configuration Release
dotnet test LibraryDesk.sln --configuration Release --no-build
```

## Run

```powershell
dotnet run --project src/LibraryDesk.Desktop/LibraryDesk.Desktop.csproj
```

The application opens a desktop navigation shell. Each navigation area clearly states the later development phase in which its real functionality will be introduced.

On first startup, the application applies its EF Core migrations and creates its local database at `%LocalAppData%\LibraryDesk\LibraryDesk.db`. To use a different location, set `Database:FilePath` in `appsettings.json` or define `LIBRARYDESK_Database__FilePath`.

## Project layout

```text
src/
  LibraryDesk.Domain/          Business primitives; no outward dependencies
  LibraryDesk.Application/     Use-case abstractions and application primitives
  LibraryDesk.Infrastructure/  Implementations of application abstractions
  LibraryDesk.Desktop/         Avalonia UI, composition root, shell and view models
tests/
  LibraryDesk.Domain.Tests/
  LibraryDesk.Application.Tests/
  LibraryDesk.Infrastructure.Tests/
docs/
```

See [architecture documentation](docs/ARCHITECTURE.md) and [development documentation](docs/DEVELOPMENT.md) for the project conventions, database decisions, and migration workflow.
