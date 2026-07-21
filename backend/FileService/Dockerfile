FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8003

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG NUGET_USERNAME
ARG NUGET_TOKEN
WORKDIR /src

COPY ["NuGet.Config", "."]
COPY ["FileService/", "FileService/"]

RUN dotnet nuget update source github \
    --username $NUGET_USERNAME \
    --password $NUGET_TOKEN \
    --store-password-in-clear-text \
    --configfile /src/NuGet.Config

RUN dotnet restore "FileService/src/FileService.Web/FileService.Web.csproj"

WORKDIR "/src/FileService/src/FileService.Web"
RUN dotnet publish "FileService.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
USER root
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FileService.Web.dll"]
