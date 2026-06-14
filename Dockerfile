# syntax=docker/dockerfile:1
# Multi-stage build for Kenman Design Studio (.NET 10 Blazor Server, 3 projects:
# Core + Infrastructure + Web). Mirrors the AWBlazor image conventions.

ARG DOTNET_VERSION=10.0

# ---- build ---------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Restore as a cache-friendly layer — copy only the three .csproj first.
# The Web project references Core + Infrastructure via relative paths, so
# restoring Web pulls all three.
COPY src/KenmanDesignStudio.Core/KenmanDesignStudio.Core.csproj                     KenmanDesignStudio.Core/
COPY src/KenmanDesignStudio.Infrastructure/KenmanDesignStudio.Infrastructure.csproj KenmanDesignStudio.Infrastructure/
COPY src/KenmanDesignStudio.Web/KenmanDesignStudio.Web.csproj                       KenmanDesignStudio.Web/
RUN dotnet restore KenmanDesignStudio.Web/KenmanDesignStudio.Web.csproj

# Copy the rest of the source and publish the web app.
COPY src/KenmanDesignStudio.Core/           KenmanDesignStudio.Core/
COPY src/KenmanDesignStudio.Infrastructure/ KenmanDesignStudio.Infrastructure/
COPY src/KenmanDesignStudio.Web/            KenmanDesignStudio.Web/
WORKDIR /src/KenmanDesignStudio.Web
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ---- runtime -------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final
WORKDIR /app

# ICU for MudBlazor/ApexCharts culture formatting; tzdata for correct timestamps.
RUN apt-get update \
 && apt-get install -y --no-install-recommends tzdata libicu-dev \
 && rm -rf /var/lib/apt/lists/*

# Non-root user for the app process.
RUN groupadd -r kds && useradd -r -g kds -m -d /home/kds kds

COPY --from=build /app/publish .

# App_Data holds DataProtection keys (Blazor antiforgery) — keep it writable + persistent.
RUN mkdir -p /app/App_Data && chown -R kds:kds /app

USER kds

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080
ENTRYPOINT ["dotnet", "KenmanDesignStudio.Web.dll"]
