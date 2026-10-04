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
- Keep database and filesystem access in Infrastructure. Phase 1 includes no persistence.
- Add tests in the test project corresponding to the layer under test. Tests must not depend on the Avalonia desktop UI unless a later phase requires dedicated UI testing.

## Phase 1 boundaries

This phase deliberately does not include data models, a database, credentials, readers, titles, copies, borrowing, reporting, backup, or synchronization. The desktop shell is a real startable application, but all feature pages are transparent placeholders for their assigned future phase.
