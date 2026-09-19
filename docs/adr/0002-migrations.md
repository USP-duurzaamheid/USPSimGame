# ADR 0002: Keep EF Core migrations in Infrastructure

- Status: Accepted
- Date: 2026-09-19

## Decision

EF Core migrations live in `USPSimGame.Infrastructure`. Npgsql is configured with that assembly explicitly because `AppDbContext` lives in Application:

```text
MigrationsAssembly(typeof(InfrastructureMarker).Assembly.FullName)
```

Use these commands from the repository root:

```bash
dotnet ef migrations add <Name> --project src/USPSimGame.Infrastructure --startup-project src/USPSimGame.Web
dotnet ef database update --project src/USPSimGame.Infrastructure --startup-project src/USPSimGame.Web
```

Migration namespaces may change without changing migration IDs stored in `__EFMigrationsHistory`. Startup migration failures are retried and then surfaced; the application must never infer permission to delete a database from an error string.

## Optional future squash

Only squash migrations after the team explicitly confirms that deployed migration history is disposable. Take and verify a backup, create a replacement initial migration against an empty database, and test both new-database creation and the agreed transition for existing environments. No squash is part of this refactor.
