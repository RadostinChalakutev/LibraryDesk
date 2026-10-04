# Windows Release Instructions

## Produce the release

From the repository root, run:

```powershell
dotnet restore LibraryDesk.sln
dotnet test LibraryDesk.sln --configuration Release
dotnet publish src/LibraryDesk.Desktop/LibraryDesk.Desktop.csproj --configuration Release --runtime win-x64 --self-contained true --output release
```

The publish directory contains `LibraryDesk.Desktop.exe` and its runtime dependencies. Run the executable directly to smoke-test the package. The application creates its database and log folders per Windows user on first run.

## Distribution notes

- The produced package is a portable self-contained Windows x64 deployment; it does not require a separate .NET runtime install.
- Copy the complete `release` directory, not only the `.exe` file.
- No installer, code-signing certificate, auto-update service, or in-place restore feature is included in this repository.
- For organizational deployment, sign the executable and package it with your chosen installer/MSIX tooling after validating the release in a representative Windows environment.
