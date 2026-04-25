# =============================================================================
# CLS Backend — Production Dockerfile
# Multi-stage build: SDK (build) → ASP.NET Runtime (run)
# Built from repository root: Developments/CLS/
# =============================================================================

# ---------------------------------------------------------------------------
# Stage 1: BUILD — Restore, compile, and publish the CLS backend
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /src

# ── 1a. Copy solution and all .csproj files first (dependency caching) ─────
# Docker caches this layer. NuGet restore only re-runs when project
# references or package versions change — not on every source code edit.
COPY CLS.slnx ./
COPY CLS.Server/CLS.Server.csproj    CLS.Server/
COPY CLS.BLL/CLS.BLL.csproj          CLS.BLL/
COPY CLS.DAL/CLS.DAL.csproj          CLS.DAL/

# ── 1b. Restore NuGet packages (cached independently from source) ──────────
# Ignore the cls.client esproj — frontend is built/deployed separately.
RUN dotnet restore CLS.Server/CLS.Server.csproj \
    --runtime linux-musl-x64

# ── 1c. Copy remaining backend source code ─────────────────────────────────
COPY CLS.Server/ CLS.Server/
COPY CLS.BLL/    CLS.BLL/
COPY CLS.DAL/    CLS.DAL/

# ── 1d. Publish in Release mode ────────────────────────────────────────────
# --no-restore : packages already restored above
# /p:UseAppHost=false : skip native executable; the app runs via `dotnet`
RUN dotnet publish CLS.Server/CLS.Server.csproj \
    --configuration Release \
    --runtime linux-musl-x64 \
    --no-restore \
    --self-contained false \
    /p:UseAppHost=false \
    --output /app/publish

# ---------------------------------------------------------------------------
# Stage 2: RUNTIME — Minimal ASP.NET Core image
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime

# ── 2a. Install runtime dependencies (not included in Alpine) ──────────────
# icu-libs:  Unicode/globalization support (date, number, locale formatting)
# krb5-libs: Kerberos/GSSAPI — required by Npgsql for PostgreSQL connections
# wget:      Docker HEALTHCHECK probe
RUN apk add --no-cache icu-libs krb5-libs wget

# ── 2b. Create non-root user and group for security ────────────────────────
# Running as root inside containers is a security anti-pattern.
RUN addgroup --system --gid 1001 clsgroup \
 && adduser  --system --uid 1001 --ingroup clsgroup --shell /sbin/nologin clsuser

WORKDIR /app

# ── 2c. Copy published output from build stage ─────────────────────────────
COPY --from=build --chown=clsuser:clsgroup /app/publish .

# ── 2d. Environment variables — ASP.NET Core production defaults ───────────
# PORT: Render.com sets this to 10000; docker-compose uses 8080 (default)
ENV ASPNETCORE_ENVIRONMENT=Production \
    PORT=8080 \
    # Disable .NET telemetry and invariant globalization for Alpine
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# ── 2e. Expose application port ────────────────────────────────────────────
EXPOSE 8080 10000

# ── 2f. Switch to non-root user ────────────────────────────────────────────
USER clsuser

# ── 2g. Docker health check — lightweight HTTP probe ───────────────────────
# Shell form allows $PORT variable interpolation at runtime.
HEALTHCHECK --interval=30s --timeout=5s --retries=3 --start-period=10s \
    CMD wget --no-verbose --tries=1 --spider http://localhost:$PORT/health || exit 1

# ── 2h. Start the application ──────────────────────────────────────────────
# Shell form: dynamically sets ASPNETCORE_URLS from PORT env var at runtime.
# Render.com overrides PORT=10000; docker-compose keeps PORT=8080 (default).
CMD ASPNETCORE_URLS=http://+:$PORT dotnet CLS.Server.dll

