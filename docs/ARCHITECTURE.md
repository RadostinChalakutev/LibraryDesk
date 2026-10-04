# Architecture

## Dependency direction

LibraryDesk uses a compact Clean Architecture arrangement. Project dependencies point inward:

```text
Desktop ──────► Application ──────► Domain
  │                    ▲
  └────► Infrastructure ┘
                 │
                 └───────────────► Domain
```

`LibraryDesk.Domain` has no dependency on another LibraryDesk project. `LibraryDesk.Application` contains contracts and DTOs and depends only on Domain. `LibraryDesk.Infrastructure` implements application contracts and owns EF Core, SQLite, filesystem reporting/backups, and migrations. `LibraryDesk.Desktop` contains Avalonia views, MVVM view models, and the single composition root.

## Responsibilities

- **Domain** contains readers, books, physical-copy state, loans, roles, capabilities, and their invariants. For example, a non-available copy cannot be lent and a loan cannot be returned twice.
- **Application** contains use-case interfaces and records: readers, catalogue, circulation, dashboard, reports, account administration, backups, authentication, and `IUserSession`. It has no SQL, filesystem, or Avalonia types.
- **Infrastructure** implements those interfaces using EF Core/SQLite. It owns `LibraryDeskDbContext`, schema migrations, PBKDF2 password hashing, transactional lending, report files, and SQLite online backups.
- **Desktop** binds views to view models. It never uses `DbContext` directly; all business work passes through Application interfaces registered by `DesktopBootstrapper`.

## Workflow and persistence

The core workflow persists this relationship:

```text
Reader 1 ──< Loan >── 1 PhysicalCopy >── 1 Book
```

Each physical copy has a globally unique barcode and status. A filtered unique SQLite index allows at most one active loan for a physical copy. Borrowing and returning update both the loan record and physical-copy state in a database transaction. Returned loans remain immutable history.

The local database is `%LocalAppData%\\LibraryDesk\\LibraryDesk.db` by default. `Database:FilePath` configuration, or its `LIBRARYDESK_Database__FilePath` environment-variable form, can select a different path. `IDatabaseInitializer` applies EF Core migrations at startup; it does not seed real library records.

Current EF Core migrations are:

- `InitialInfrastructure` — metadata table.
- `AddAuthentication` — local user accounts.
- `AddReaders` — reader records.
- `AddBooksAndCopies` — catalogue titles and physical copies.
- `AddLoans` — circulation history and active-loan index.

## Authentication and authorization

The initial setup flow atomically creates the first administrator, with no default account. Passwords are verified with PBKDF2-HMAC-SHA512 using 210,000 iterations, a cryptographically random 16-byte salt, and a 32-byte derived key. Plaintext passwords are not persisted.

`UserRole` and `Capability` are Domain concepts. The shell hides unavailable modules, and user-administration services also validate the authenticated administrator role before modifying accounts. Librarians can use operational modules but cannot see or invoke account administration.

## Presentation model

Avalonia follows MVVM:

- XAML views contain bindings and layout only.
- View models own view state and commands.
- Application services own workflow calls.
- `AsyncRelayCommand` prevents a command from running concurrently with itself.

The main shell makes Dashboard, Readers, Books, Loans, Overdue, Reports, and Settings available according to permissions.

## Cross-cutting decisions

- Nullable reference types, implicit usings, current analyzer recommendations, and warnings-as-errors are enabled centrally in `Directory.Build.props`.
- Serilog creates rolling daily logs in `%LocalAppData%\\LibraryDesk\\logs`.
- CSV reports quote fields and prefix formula-leading user text with an apostrophe to avoid spreadsheet formula injection.
- Database backups use SQLite's backup API rather than copying a potentially live `.db` file.
