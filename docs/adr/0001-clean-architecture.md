# ADR 0001: Enforce application boundaries with four projects

- Status: Accepted
- Date: 2026-09-19

## Context

The application previously compiled as one ASP.NET Core project. Domain entities, EF Core data access, application services, infrastructure integrations, and Blazor UI code could therefore depend on each other without compiler-enforced boundaries.

## Decision

Split the solution into an acyclic dependency graph:

```text
Domain <- Application <- Infrastructure
              ^              ^
              +----- Web ----+
```

- Domain contains entities, enums, and pure utilities and has no package or project references.
- Application contains `AppDbContext`, application models, and services. It may use EF Core abstractions but not a database provider.
- Infrastructure selects PostgreSQL/Npgsql, owns migrations, and implements ASP.NET-coupled services.
- Web is the composition root and contains the Blazor UI and HTTP endpoints.

Application deliberately retains the EF-touching service implementations. Moving those implementations behind repositories would turn this restructure into a behavioral rewrite. The rejected alternative is an `IAppDbContext` plus custom factory abstraction, which would add widespread constructor churn without changing current behavior.

## Consequences

The compiler now prevents Domain from reaching framework and UI concerns, and Application cannot choose an Npgsql provider. Application is not EF-free; extracting persistence ports remains a possible later refactor. Additional architecture rules can be added as the test suite grows.
