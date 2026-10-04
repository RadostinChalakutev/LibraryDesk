# LibraryDesk

LibraryDesk is a Windows x64 desktop application for small-library circulation. It supports the core operational workflow:

```text
Reader → Book → Physical copy → Borrow → Due date → Return → History
```

The application is built with C#/.NET 10, Avalonia UI, MVVM, EF Core, SQLite, dependency injection, and Serilog.

## Features

- Secure first-run administrator setup, local sign-in/sign-out, and administrator/librarian permissions.
- Reader registration, search, editing, activation, and deactivation.
- Catalogue title creation, title search, barcode-addressable physical-copy registration, and availability counts.
- Transactional borrowing and returns, current-loan view, copy availability protection, and loan history.
- Overdue queue, dashboard counts, on-screen circulation history, and a spreadsheet-safe CSV export.
- Administrator-managed local staff accounts and consistent SQLite database backups.
- Automatic EF Core database migrations at application startup. No default user account or sample data is created.

## Prerequisites

- Windows 10 or later (x64)
- [.NET SDK 10.0.400](https://dotnet.microsoft.com/download/dotnet/10.0) or a compatible .NET 10 SDK for development

## Build, test, and run

Run these from the repository root:

```powershell
dotnet restore LibraryDesk.sln
dotnet build LibraryDesk.sln --configuration Release
dotnet test LibraryDesk.sln --configuration Release --no-build
dotnet run --project src/LibraryDesk.Desktop/LibraryDesk.Desktop.csproj --configuration Release
```

On first launch, LibraryDesk creates and migrates `%LocalAppData%\\LibraryDesk\\LibraryDesk.db` then requires the operator to create the first administrator. There are deliberately no default credentials.

Set `LIBRARYDESK_Database__FilePath` to use an alternative SQLite database path, for example during development:

```powershell
$env:LIBRARYDESK_Database__FilePath = "$PWD\\local-data\\LibraryDesk.db"
dotnet run --project src/LibraryDesk.Desktop/LibraryDesk.Desktop.csproj
```

## Windows release

Create a self-contained x64 release in `release/` with:

```powershell
dotnet publish src/LibraryDesk.Desktop/LibraryDesk.Desktop.csproj --configuration Release --runtime win-x64 --self-contained true --output release
```

Run `release\\LibraryDesk.Desktop.exe`. See [release instructions](docs/RELEASE.md) for packaging notes.

## Project layout

```text
src/
  LibraryDesk.Domain/          Business entities, invariants, roles, and capabilities
  LibraryDesk.Application/     Use-case contracts, DTOs, and application session state
  LibraryDesk.Infrastructure/  SQLite/EF Core, service implementations, migrations, files
  LibraryDesk.Desktop/         Avalonia views, MVVM view models, DI composition root
tests/
  LibraryDesk.Domain.Tests/
  LibraryDesk.Application.Tests/
  LibraryDesk.Infrastructure.Tests/
docs/
  ARCHITECTURE.md
  DEVELOPMENT.md
  USER_GUIDE.md
  RELEASE.md
```

See [architecture](docs/ARCHITECTURE.md), [development](docs/DEVELOPMENT.md), [user guide](docs/USER_GUIDE.md), and [release](docs/RELEASE.md).
