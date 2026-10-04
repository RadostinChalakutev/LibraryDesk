# Architecture

## Overview

LibraryDesk uses a small Clean Architecture arrangement. Dependencies point inward:

```text
Desktop -> Infrastructure -> Application -> Domain
                 \---------------> Domain
```

The Desktop project also references Application so that it can compose application abstractions at its startup boundary. Domain never references another LibraryDesk project. Application references only Domain. Infrastructure supplies implementations for abstractions owned by Application.

## Layer responsibilities

- **Domain** holds dependency-free business concepts and invariants. Phase 1 establishes a reusable date-range primitive; database entities are intentionally deferred.
- **Application** owns use-case contracts, application-level primitives, and service registration entry points. It has no UI or persistence code.
- **Infrastructure** implements application contracts for external concerns. Phase 1 provides the system clock only; persistence begins in Phase 2.
- **Desktop** is the Avalonia presentation layer and composition root. It builds configuration, logging and the DI container, then resolves the window view model.

## Presentation model

The UI follows MVVM. Views contain layout and bindings only. `MainWindowViewModel` owns the navigation state; one intentionally non-functional placeholder page view model represents each future feature area. This avoids creating fake reader, book, or lending functionality before the relevant phase.

## Cross-cutting concerns

- Nullable references and implicit global usings are enabled project-wide in `Directory.Build.props`.
- Serilog writes rolling daily logs to `%LocalAppData%\LibraryDesk\logs`.
- `appsettings.json` is copied to the desktop output and may be overridden with `LIBRARYDESK_`-prefixed environment variables (for example, `LIBRARYDESK_Logging__MinimumLevel=Debug`).
- The desktop project targets `win-x64` and creates a Windows executable host.
