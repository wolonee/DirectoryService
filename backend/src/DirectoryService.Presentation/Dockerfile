FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DirectoryService.Presentation/DirectoryService.Presentation.csproj", "DirectoryService.Presentation/"]
COPY ["DirectoryService.Application/DirectoryService.Application.csproj", "DirectoryService.Application/"]
COPY ["DirectoryService.Contracts/DirectoryService.Contracts.csproj", "DirectoryService.Contracts/"]
COPY ["DirectoryService.Domain/DirectoryService.Domain.csproj", "DirectoryService.Domain/"]
COPY ["DirectoryService.Shared.Kernel/DirectoryService.Shared.Kernel.csproj", "DirectoryService.Shared.Kernel/"]
COPY ["DirectoryService.Infrastructure.Postgres/DirectoryService.Infrastructure.Postgres.csproj", "DirectoryService.Infrastructure.Postgres/"]

RUN dotnet restore "DirectoryService.Presentation/DirectoryService.Presentation.csproj"
COPY . .
WORKDIR "/src/src/DirectoryService.Presentation"
RUN dotnet build "./DirectoryService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DirectoryService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DirectoryService.Presentation.dll"]
