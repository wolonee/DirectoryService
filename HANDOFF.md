# HANDOFF — Docker Compose containerization

Session goal: bring the whole stack (`postgres`, `directory-service`, `frontend`, `nginx`) up with a single `docker compose up`, one shared network, secrets in `.env`. Not a production deploy — local dev convenience stack only (no TLS, no prod secrets).

## Current state: working, verified end-to-end

Confirmed manually via `curl`/browser during this session:
- `docker compose up -d --build` builds and starts all 5 services.
- `postgres` has a `pg_isready` healthcheck; `directory-service` and `directory-service-migrations` wait on it (`condition: service_healthy`).
- `directory-service-migrations` is a one-shot container (`restart: "no"`) that runs a `dotnet ef migrations bundle` executable (`efbundle`) against Postgres, then exits 0. `directory-service` waits on `condition: service_completed_successfully` before starting — migrations always run before the API.
- `directory-service` listens on port **8002** inside the container (not the base image's default 8080 — forced via `ASPNETCORE_HTTP_PORTS=8002`), reachable directly at `http://localhost:8002` and Swagger UI at `http://localhost:8002/` (root, not `/swagger` — see below).
- `nginx` reverse-proxies `http://localhost/api/*` → strips the `/api` prefix → `directory-service:8002/*`. Verified with `curl http://localhost/api/locations` returning real data.
- `frontend` builds via multi-stage Dockerfile (`node:22-alpine`, Next.js `output: "standalone"`), served on `http://localhost:3000`.

## Known-good file states (for reference / diffing later)

- `docker-compose.yml` — 5 services + `postgres-data` volume. All secrets/config pulled from root `.env` via `${VAR}` interpolation (build args, `environment:`, healthcheck `test:`, entrypoint strings — interpolation works everywhere in the YAML, not just `environment:`).
- `.env` (gitignored) / `.env.example` (committed) — keys: `NUGET_USERNAME`, `NUGET_TOKEN` (GitHub Packages feed creds for the private NuGet feed), `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `DB_CONNECTION_STRING` (whole connection string, passed as one var — note this duplicates the three `POSTGRES_*` values; if you rotate the Postgres password, update both), `NEXT_PUBLIC_API_URL`.
- `nginx.conf` — single `location /api/ { proxy_pass http://directory-service:8002/; ... }`. Both sides need the trailing slash — that's what makes nginx strip the `/api/` prefix before forwarding.
- `backend/DirectoryService/Dockerfile` — multi-stage: `sdk:9.0` build stage installs `dotnet-ef` as a global tool, builds an `efbundle` executable, then `publish`; `aspnet:9.0` final stage copies both the app and the bundle (`--chown=$APP_UID:$APP_UID` on the bundle, needed because the final stage runs as a non-root user and can't `chmod` a root-owned file).
- `frontend/Dockerfile` — multi-stage `node:22-alpine`; `NEXT_PUBLIC_API_URL` is a build `ARG` (not just `ENV` — Next.js inlines `NEXT_PUBLIC_*` vars into the client bundle at `npm run build` time, so it must be passed as `--build-arg`/compose `build.args`, not `env_file`/runtime env).
- Backend controllers have **no** `/api` route prefix (`[Route("locations")]`, not `[Route("api/locations")]`) — this was an intentional choice made this session, not a bug. `/api` is purely an nginx-gateway-level contract. `CLAUDE.md` / `backend/CLAUDE.md` were updated to reflect this.
- `Program.cs`: Swagger now gated on `!app.Environment.IsProduction()` (was `IsDevelopment()`) so it's reachable when `ASPNETCORE_ENVIRONMENT=Docker`. CORS policy allows `http://localhost:3000` and `http://frontend:3000`.
- `appsettings.Docker.json`: `ConnectionStrings:DirectoryServiceDb` now points at `postgres:5432` (container-internal port, not the `5434` host-mapped port — this was the very first bug of the session and is the one to remember: **host-mapped ports and container-internal ports are different numbers; container-to-container traffic always uses the internal port**).

## Bugs found + fixed this session (chronological, for context if something regresses)

1. `Directory.Build.Props` (capital P) — MSBuild's implicit import looks for lowercase `Directory.Build.props`; worked on macOS (case-insensitive FS), broke in the Linux container. Fixed by renaming the file in the repo (not a Docker workaround).
2. Dockerfile `COPY`/restore paths missing the project's own `src/` segment (`DirectoryService/src/DirectoryService.Presentation/...`, not `DirectoryService/DirectoryService.Presentation/...`).
3. `backend/.dockerignore` originally excluded `nuget.config` — broke private NuGet feed restore inside the build context. Removed that exclusion (context is `./backend`, so `.dockerignore` must live at `backend/.dockerignore`, not one level down).
4. `ARG NUGET_USERNAME`/`NUGET_TOKEN` were declared in the wrong Dockerfile stage (`base` instead of `build`, where they're actually used) — Docker `ARG`s don't cross stage boundaries.
5. `ENV PATH ="..."` (space before `=`) — invalid Docker `ENV` syntax, silently corrupted `PATH` instead of erroring.
6. `dotnet ef migrations bundle` doesn't *generate* migrations, only packages existing ones — a real `PendingModelChangesWarning` (model/migration drift already present in the codebase, unrelated to Docker) had to be fixed with `dotnet ef migrations add` on the host first.
7. `docker-compose.yml`: `depends_on`/`restart` were nested inside `build:` by mistake (YAML indentation) — moved to the service level.
8. `postgres` had no `healthcheck`, but another service depended on `condition: service_healthy` — added `pg_isready`-based healthcheck.
9. App listens on 8080 by default (base image default `ASPNETCORE_HTTP_PORTS`), compose mapped `8002:8002` — nothing was listening on the container's 8002. Fixed by setting `ASPNETCORE_HTTP_PORTS=8002` explicitly rather than changing the port mapping.
10. Swagger 404 — `app.Environment.IsDevelopment()` is `false` when `ASPNETCORE_ENVIRONMENT=Docker`, so `UseSwagger()` never ran. Changed to `!IsProduction()`.
11. `nginx.conf` had a typo (`includde`) — config failed to parse, nginx crash-looped.
12. `frontend` service pointed at `./client` (doesn't exist) instead of `./frontend`.
13. `node:latest-alpine` isn't a real tag on Docker Hub — pinned to `node:22-alpine`.
14. `.next/standalone` didn't exist — `next.config.ts` was missing `output: "standalone"`.
15. Backend routes lost their `/api` prefix at some point (or it was deliberately removed this session) — nginx was passing `/api/...` through unstripped to a backend that doesn't have that prefix. Fixed nginx to strip it (`location /api/` + trailing-slash `proxy_pass`).
16. **Most recent, just fixed**: frontend POST requests were hitting `http://localhost:3000/locations` (the frontend's own origin) instead of the backend, causing `405`. Root cause: `frontend/Dockerfile` did `ENV NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL` with no matching `ARG` declared in that stage, so the value baked into the build was an empty string, not the intended URL — and axios's `?? "http://localhost/api"` fallback doesn't trigger on `""` (only `null`/`undefined`). Fixed by adding `ARG NEXT_PUBLIC_API_URL` to the Dockerfile and `build.args` in compose.

## Not yet verified

- Fix #16 (`NEXT_PUBLIC_API_URL` build-arg) was applied but **`docker compose up -d --build frontend` has not been re-run/re-tested yet** — this is the immediate next step when resuming.
- Full `docker compose down -v && docker compose up -d --build` clean-slate run (all 5 services from a fresh volume) hasn't been done since the frontend service and the `/api`-prefix nginx fix landed — worth doing once #16 is confirmed, to make sure nothing regressed.
- Cosmetic, not blocking: `backend/DirectoryService/Dockerfile` still has `EXPOSE 8001` (stale — real port is 8002; `EXPOSE` is documentation-only, doesn't affect runtime).
- Not investigated this session: `.github/workflows/dotnet.yml` has stale paths from an earlier repo restructure (`backend/DirectoryService/` + `backend/Shared/` split) — CI likely broken, out of scope for this containerization task.

## How to resume

```
cp .env.example .env   # fill in real NUGET_USERNAME/NUGET_TOKEN
docker compose up -d --build
docker compose ps                                   # all healthy/running?
curl http://localhost/api/locations                  # backend via nginx
open http://localhost:3000                           # frontend, try creating a location (was the failing case)
docker compose logs -f directory-service              # if anything 500s
```
