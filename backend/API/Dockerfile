# =============================================================================
# RoKey Management API - Dockerfile Multi-stage
# =============================================================================
# Build stage: Compila la aplicación
# Runtime stage: Imagen ligera solo con runtime y la app compilada
# =============================================================================

# -----------------------------------------------------------------------------
# BUILD STAGE
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto primero (para cache de capas)
COPY ["API.csproj", "./"]
RUN dotnet restore "API.csproj"

# Copiar todo el código y compilar
COPY . .
RUN dotnet publish "API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# -----------------------------------------------------------------------------
# RUNTIME STAGE
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Usuario no-root para seguridad
RUN adduser --disabled-password --gecos "" --uid 1000 appuser \
    && chown -R appuser:appuser /app
USER appuser

# Copiar app compilada desde build stage
COPY --from=build --chown=appuser:appuser /app/publish .

# Puerto expuesto (Cloud Run usa PORT env var, default 8080)
EXPOSE 8080

# Health check endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Variables de entorno por defecto (sobrescribibles en runtime)
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

# Entry point
ENTRYPOINT ["dotnet", "API.dll"]