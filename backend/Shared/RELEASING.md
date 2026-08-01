# Выпуск новой версии Shared-пакета

Shared содержит три отдельных NuGet-пакета:

| Папка | Package ID |
|---|---|
| `Shared.SharedKernel` | `DirectoryService.SharedKernel` |
| `Core` | `DirectoryService.Core` |
| `Framework` | `DirectoryService.Framework` |

Если изменён только один пакет, выпускать остальные не нужно. Уже опубликованную версию перезаписать нельзя — перед каждым релизом увеличивай `<Version>` в `.csproj`.

## Пример: изменён только Framework

Перейди прямо в его папку:

```bash
cd backend/Shared/Framework
```

### 1. Увеличь версию

В `Framework.csproj`, например:

```xml
<Version>0.0.2</Version>
```

### 2. Проверь сборку

```bash
dotnet build -c Release
```

### 3. Отправь исходники в GitHub

Поскольку ты находишься в папке `Framework`, команда `git add .` добавит только её изменения:

```bash
git add .
git commit -m "release Framework 0.0.2"
git push
```

### 4. Создай NuGet-пакет

```bash
dotnet pack -c Release --no-restore -o ../artifacts
```

Пакет появится здесь:

```text
backend/Shared/artifacts/DirectoryService.Framework.0.0.2.nupkg
```

### 5. Опубликуй пакет

Токену GitHub нужны права `write:packages` и `read:packages`.

```bash
read -s "GITHUB_PACKAGES_TOKEN?GitHub Packages token: "
echo

dotnet nuget push ../artifacts/DirectoryService.Framework.0.0.2.nupkg \
  --source github \
  --api-key "$GITHUB_PACKAGES_TOKEN" \
  --skip-duplicate

unset GITHUB_PACKAGES_TOKEN
```

Источник `github` уже настроен в `backend/nuget.config`. Этот файл содержит credentials и не должен попадать в Git.

### 6. Обнови FileService

Перейди из `Shared/Framework` в FileService:

```bash
cd ../../FileService
```

Поменяй версию `DirectoryService.Framework` на `0.0.2` в проектах, которые его используют:

- `src/FileService.Core/FileService.Core.csproj`;
- `src/FileService.Web/FileService.Web.csproj`.

Затем:

```bash
dotnet restore FileService.sln --force-evaluate
dotnet build FileService.sln --no-restore
```

## Для Core или SharedKernel

Алгоритм тот же: перейди в нужную папку и замени имя пакета и версию в командах.

```bash
cd backend/Shared/Core
```

или:

```bash
cd backend/Shared/Shared.SharedKernel
```

Если изменён `SharedKernel`, сначала опубликуй его. Новые версии `Core` и `Framework` нужны только тогда, когда они должны зависеть от новой версии Kernel.

## Коротко

```text
увеличить Version
→ dotnet build
→ git add/commit/push
→ dotnet pack
→ dotnet nuget push
→ обновить PackageReference у потребителя
→ dotnet restore/build
```
