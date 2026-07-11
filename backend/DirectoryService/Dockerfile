FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG NUGET_USERNAME
ARG NUGET_TOKEN

WORKDIR /src

COPY ["nuget.config", "."]
COPY [".editorconfig", "."]
COPY ["DirectoryService/", "DirectoryService/"]

RUN dotnet nuget update source github \
    --username $NUGET_USERNAME \
    --password $NUGET_TOKEN \
    --store-password-in-clear-text \
    --configfile /src/nuget.config

RUN dotnet restore "DirectoryService/src/DirectoryService.Presentation/DirectoryService.Presentation.csproj"

RUN dotnet tool install --global dotnet-ef --version 9.0.0
ENV PATH="/root/.dotnet/tools:${PATH}"

WORKDIR "/src/DirectoryService/src/DirectoryService.Presentation"

RUN dotnet ef migrations bundle \
    --project "../DirectoryService.Infrastructure.Postgres/DirectoryService.Infrastructure.Postgres.csproj" \
    --startup-project "DirectoryService.Presentation.csproj" \
    --output /app/efbundle \
    --configuration Release \
    --verbose \
    -- --configfile /src/nuget.config

RUN dotnet build "./DirectoryService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DirectoryService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
USER root
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app

COPY --from=publish /app/publish .
COPY --chown=$APP_UID:$APP_UID --from=build /app/efbundle .
RUN chmod +x efbundle

EXPOSE 8002
ENTRYPOINT ["dotnet", "DirectoryService.Presentation.dll"]
