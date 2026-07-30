# File Service integration tests

Run from `backend/FileService`:

```bash
dotnet test tests/FileService.IntegrationTests/FileService.IntegrationTests.csproj
```

Docker daemon must be running. Testcontainers starts PostgreSQL and MinIO itself; local `docker compose up`, local Postgres and local MinIO are not used.

For CI, run same command on a runner with Docker access. Do not skip `DockerUnavailableException`: it means test environment cannot verify storage flow.
