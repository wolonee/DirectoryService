# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository layout

Three independent units, each with its own toolchain:

- `backend/` — .NET 9 Web API (`DirectoryService.sln`), Clean Architecture + CQRS.
- `frontend/` — Next.js 16 / React 19 app (Feature-Sliced Design). **Has its own `frontend/AGENTS.md` — read it before writing frontend code; Next.js 16 has breaking changes and that file points to the bundled docs.**
- `Shared/` — a *separate* .NET solution (`Shared.sln`) holding the cross-cutting kernel (CQRS abstractions, validation helpers, base HTTP client) under namespace `DirectoryService.Application.Abstractions` / `DirectoryService.Shared.*`. The backend projects reference these.

There are `node_modules` + `package.json` at **both** the repo root and `frontend/`. Always install frontend packages from inside `frontend/` (a dependency installed at the root will not resolve in the Next.js build).

## Commands

### Backend (run from `backend/`)
- Start dependencies: `docker compose up -d` — Postgres on `localhost:5434`, Seq (logs) on `http://localhost:8081`.
- Build: `dotnet build`
- Run API: `dotnet run --project src/DirectoryService.Presentation` — serves on `http://localhost:5057`, Swagger at `/swagger`, all routes under `/api`.
- All tests: `dotnet test tests/DirectoryService.IntegrationTests` — **requires Docker** (Testcontainers spins up a throwaway Postgres).
- Single test / class: `dotnet test tests/DirectoryService.IntegrationTests --filter "FullyQualifiedName~GetDepartmentsTests"`
- Migrations: `dotnet ef migrations add <Name> --project src/DirectoryService.Infrastructure.Postgres --startup-project src/DirectoryService.Presentation` (and `database update`). Migrations are **not** auto-applied at startup.

StyleCop analyzers are enabled repo-wide (`Directory.Build.Props`); the build emits many style warnings — that is the existing baseline, not something to fix wholesale.

### Frontend (run from `frontend/`)
- Dev server: `npm run dev` (expects the API at `http://localhost:5057/api`)
- Build: `npm run build` · Lint: `npm run lint` · Typecheck: `npx tsc --noEmit`

## Backend architecture

**CQRS, no MediatR.** Commands/queries implement marker interfaces `ICommand`/`IQuery`; handlers implement `ICommandHandler<TResp,TCmd>` / `ICommandHandler<TCmd>` / `IQueryHandler<TResp,TQuery>` (defined in `Shared/Core/Abstractions`). Handlers are auto-registered by Scrutor assembly scan via `services.AddHandlers(assembly)` in `Application/DependencyInjection.cs` — **never register a handler manually**. Controllers receive the handler through `[FromServices] IQueryHandler<...>` and call `handler.Handle(query, ct)`.

**Result-based error flow, no exceptions for control.** Handlers return `Result<T, Errors>` / `UnitResult<Errors>` (CSharpFunctionalExtensions). `Errors` is a custom aggregate; `.ToErrors()` / `.ToValidationErrors()` convert domain errors and FluentValidation results. The API serializes everything into an `Envelope<T>` (`{ result, errors }`); `ExceptionMiddleware` + `ErrorsJsonConverter` handle the wire format. FluentValidation validators are also auto-registered and run at the top of each handler.

**Layer dependencies:** `Presentation → Application → Domain`; `Contracts` holds request/response/DTO records shared between API and clients; `Infrastructure.Postgres` provides EF Core + repositories.

**Reads vs writes are split:**
- **Writes** go through EF Core + the domain. Domain entities (`Location`, `Department`, `Position`) have private constructors and static `Create(...)` factories returning `Result`, with value objects (`LocationName`, `DepartmentIdentifier`, …). Deletion is soft (`IsDeleted`), plus an `IsActive` flag.
- **Reads** use **Dapper raw SQL** via `IDbConnectionFactory` (note: it returns the *DbContext's own* connection, so a plain `using`/no-`using` is used — `await using` won't compile because the static type is `IDbConnection`). Paged list endpoints follow a fixed pattern: `COUNT(*) OVER() AS total_count` in the SELECT, multi-map `QueryAsync<Dto, long, Dto>` with `splitOn: "total_count"`, plus `ORDER BY … LIMIT @page_size OFFSET @offset`. Copy an existing handler (e.g. `GetDepartmentsHandler`) when adding a new one.

**Postgres specifics:** the schema relies on the `ltree` extension (department `path` hierarchy) and `pg_trgm` (trigram GIN indexes for `ILIKE` search). Because the integration-test harness builds the schema with `EnsureCreatedAsync` (model-based, not migrations), **any required Postgres extension must be declared in `DirectoryServiceDbContext.OnModelCreating` via `HasPostgresExtension(...)`** — declaring it only in a migration makes prod work but breaks tests.

## Frontend architecture

**Feature-Sliced Design** with strict downward imports `app → features → entities → shared`:
- `app/` — Next.js routing. Keep pages as **thin server components** that render a feature (e.g. `page.tsx` → `<DepartmentsList />`); push `"use client"` and hooks down into `features/`.
- `entities/<domain>/` — `api.ts` (axios calls + TanStack `queryOptions`/`infiniteQueryOptions`), `types.ts`, `model/`. One slice per backend domain (locations, departments, positions).
- `features/<domain>/` — dialogs, forms, lists, and their `model/` hooks/stores.
- `shared/` — UI kit (`components/ui`, shadcn via the `radix-ui` umbrella package), axios instance, hooks.

**Server state = TanStack Query.** Each entity exposes `queryOptions`; lists use `useInfiniteQuery` with a `select` that flattens pages and de-dupes by id. `QueryClient` is configured with `staleTime: 5min` and `refetchOnWindowFocus: false`. **Critical rule:** every value used inside `queryFn` must also appear in `queryKey`, or the query won't refetch when it changes.

**UI/filter state = Zustand** (per-feature filter stores, e.g. `departments-filter-store.ts`), exposed via a `useGet…Filter()` selector (with `useShallow`) plus standalone setter functions. **Forms = React Hook Form + Zod** (`zodResolver`); toasts via `sonner`.

**Talking to the API:** query-string param keys must match the backend C# request-record property names, which are **PascalCase** — e.g. `Search`, `IsActive`, `SortBy`, and nested `Pagination.Page` / `Pagination.PageSize`. The axios baseURL already includes `/api`.
