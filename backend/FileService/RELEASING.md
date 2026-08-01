# File Service NuGet release

Contracts are published before the client because the client package depends on a specific contracts version.

1. Increase `<Version>` in `src/FileService.Contracts/FileService.Contracts.csproj`.
2. Update the Contracts dependency used by `src/FileService.Communications/FileService.Communications.csproj` when that dependency is represented by a package version.
3. Increase `<Version>` in `src/FileService.Communications/FileService.Communications.csproj`.
4. Build and test the solution.
5. Pack both projects into `src/artifacts`.
6. Push Contracts first, then Client.
7. Update the explicit package version in each consumer.

```bash
dotnet restore FileService.sln
dotnet build FileService.sln --no-restore
dotnet test FileService.sln --no-build --no-restore

dotnet pack src/FileService.Contracts/FileService.Contracts.csproj \
  -c Release --no-restore -o src/artifacts

dotnet pack src/FileService.Communications/FileService.Communications.csproj \
  -c Release --no-restore -o src/artifacts

dotnet nuget push src/artifacts/Wolonee.FileService.Contracts.<version>.nupkg \
  --source github --skip-duplicate

dotnet nuget push src/artifacts/Wolonee.FileService.Client.<version>.nupkg \
  --source github --skip-duplicate
```

Never commit `nuget.config`, GitHub tokens, API keys, or feed credentials. Published package versions are immutable; publish a new version instead of replacing an existing package.
