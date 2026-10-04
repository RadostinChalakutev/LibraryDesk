# Architecture

## Overview

LibraryDesk uses a small Clean Architecture arrangement. Dependencies point inward:

```text
Desktop -> Infrastructure -> Application -> Domain
                 \---------------> Domain
```

The Desktop project also references Application so that it can compose application abstractions at its startup boundary. Domain never references another LibraryDesk project. Application references only Domain. Infrastructure supplies implementations for abstractions owned by Application.

## Layer responsibilities

- **Domain** holds dependency-free business concepts and invariants. It currently establishes a reusable date-range primitive; business entities are intentionally deferred.
- **Application** owns use-case contracts, application-level primitives, authentication requests/results, and in-memory desktop session state. It has no UI or persistence code.
- **Infrastructure** implements application contracts for external concerns. It provides the system clock, SQLite connection resolution, EF Core context, migrations, idempotent database initialization, persisted local accounts, and PBKDF2 password hashing.
- **Desktop** is the Avalonia presentation layer and composition root. It builds configuration, logging and the DI container, applies database migrations, then selects initial administrator setup or sign-in before opening the main shell.

## Presentation model

The UI follows MVVM. Views contain layout and bindings only. `MainWindowViewModel` owns the navigation state; one intentionally non-functional placeholder page view model represents each future feature area. This avoids creating fake reader, book, or lending functionality before the relevant phase.

## Cross-cutting concerns

- Nullable references and implicit global usings are enabled project-wide in `Directory.Build.props`.
- Serilog writes rolling daily logs to `%LocalAppData%\LibraryDesk\logs`.
- `appsettings.json` is copied to the desktop output and may be overridden with `LIBRARYDESK_`-prefixed environment variables (for example, `LIBRARYDESK_Logging__MinimumLevel=Debug`).
- SQLite data is stored in `%LocalAppData%\LibraryDesk\LibraryDesk.db` unless `Database:FilePath` is configured. SQLite foreign keys are enabled for every connection.
- `IDatabaseInitializer` is an Application-owned contract. Its Infrastructure implementation applies EF Core migrations and records the initial database creation time in the system-owned `application_metadata` table. No business data is seeded.
- First run has no default credentials. The initial setup screen creates the only bootstrap administrator through an atomic metadata record, preventing concurrent first-run setup from creating multiple bootstrap accounts.
- Passwords are hashed with PBKDF2-HMAC-SHA512 (210,000 iterations), a cryptographically random 16-byte salt, and a 32-byte derived key. Hashes and salts are Base64-encoded for SQLite storage; plaintext passwords are never persisted.
- `UserRole` and `Capability` are Domain concepts. `IUserSession` stores only the current process identity, and `RolePermissions` is the centralized authorization policy. The desktop shell filters unavailable navigation items and requires an authenticated session before it is created.
- The desktop project targets `win-x64` and creates a Windows executable host.
