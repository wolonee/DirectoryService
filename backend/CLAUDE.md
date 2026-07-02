# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

Scope: the **backend** (.NET 9 Web API, `DirectoryService.sln`). Run every command below from `backend/`.

## Commands

- **Dependencies:** `docker compose up -d` — Postgres on `localhost:5434`, Seq (logs UI) on `http://localhost:8081`.
- **Build:** `dotnet build`
- **Run API:** `dotnet run --project src/DirectoryService.Presentation` — serves `http://localhost:5057`, Swagger at `/swagger`, all routes under `/api`. `Program.cs` lives in the Presentation project (the startup project).
- **All tests:** `dotnet test tests/DirectoryService.IntegrationTests` — **requires Docker** (Testcontainers spins up a throwaway Postgres per run).
- **Single test / class:** `dotnet test tests/DirectoryService.IntegrationTests --filter "FullyQualifiedName~GetDepartmentsTests"`
- **Migrations:** `dotnet ef migrations add <Name> --project src/DirectoryService.Infrastructure.Postgres --startup-project src/DirectoryService.Presentation` (then `dotnet ef database update ...`). Migrations are **not** auto-applied at startup.

StyleCop analyzers are enabled repo-wide (`Directory.Build.props`); the build emits many style warnings — that is the existing baseline, **not** something to fix wholesale.

## Architecture

Clean Architecture + CQRS. Layer dependencies: `Presentation → Application → Domain`. `Contracts` holds request/response/DTO records shared with clients; `Infrastructure.Postgres` provides EF Core + repositories. CQRS abstractions (`ICommand`/`IQuery`, handler interfaces, `Result`/`Errors` helpers) come from the shared kernel — see the repo-root `CLAUDE.md` for the separate `Shared/` solution.

**CQRS, no MediatR.** Commands/queries implement marker interfaces `ICommand`/`IQuery`; handlers implement `ICommandHandler<TResp,TCmd>` / `ICommandHandler<TCmd>` / `IQueryHandler<TResp,TQuery>` (single-arg `IQueryHandler<TResp>` exists for parameterless queries). Handlers are auto-registered by a Scrutor assembly scan via `services.AddHandlers(assembly)` in `Application/DependencyInjection.cs` — **never register a handler manually**. Controllers receive the handler through `[FromServices] IQueryHandler<...>` and call `handler.Handle(query, ct)`.

**Result-based error flow, no exceptions for control.** Handlers return `Result<T, Errors>` / `UnitResult<Errors>` (CSharpFunctionalExtensions). `Errors` is a custom aggregate; `.ToErrors()` / `.ToValidationErrors()` convert domain errors and FluentValidation results. Controllers return `EndpointResult<T>`, which the pipeline serializes into an `Envelope<T>` (`{ result, errors }`); `ExceptionMiddleware` + `ErrorsJsonConverter` handle the wire format. FluentValidation validators are auto-registered and run at the top of each handler.

**Reads vs writes are split:**
- **Writes** go through EF Core + the domain. Domain entities (`Location`, `Department`, `Position`) have private constructors and static `Create(...)` factories returning `Result`, with value objects (`DepartmentName`, `DepartmentIdentifier`, …) whose payload is `.Value`. Deletion is soft (`IsDeleted`), plus an `IsActive` flag.
- **Reads** use **Dapper raw SQL** via `IDbConnectionFactory` (it returns the DbContext's own `IDbConnection`, so use a plain `using`/no-`using` — `await using` won't compile). Single-object reads (e.g. `GetDepartmentByIdHandler`) may use EF Core LINQ instead; Dapper is the pattern for **paged lists**.

**Paged list pattern (fixed).** Return `PaginationResponse<T>` (`Items, TotalCount, Page, PageSize, TotalPages`, namespace `DirectoryService.Contracts.Common`). In the SELECT add `COUNT(*) OVER() AS total_count`; read via multi-map `QueryAsync<Dto, long, Dto>` with `splitOn: "total_count"`; page with `ORDER BY … LIMIT @page_size OFFSET @offset`. Existence checks (e.g. parent department) run before the query so a missing id returns `404 NotFound`, not an empty page. **Copy an existing handler** (`GetChildrenByParentHandler`, `GetPositionsHandler`) when adding a new one. For bounded lists (roots, ancestors) a single full "page" is returned as `new PaginationResponse<T>(items, items.Count, 1, items.Count, 1)`.

**Postgres specifics.** The schema relies on the `ltree` extension (department `path` hierarchy) and `pg_trgm` (trigram GIN indexes for `ILIKE` search). The integration-test harness builds the schema with `EnsureCreatedAsync` (model-based, not migrations), so **any required Postgres extension must be declared in `DirectoryServiceDbContext.OnModelCreating` via `HasPostgresExtension(...)`** — declaring it only in a migration makes prod work but breaks tests. Position names are stored as jsonb; query them with `p.name->>'Speciality'` / `->>'Direction'`. Departments and positions are linked many-to-many through the `department_positions` join table.
