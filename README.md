# WhatsApp Platform

Architecture bootstrap for a multi-tenant SaaS managing multiple WhatsApp Business accounts
and attributing Meta billing under a shared partner credit line. No business features or Meta
integrations are implemented.

## Prerequisites

- .NET SDK 10.0.401 (pinned by `global.json`, latest patch allowed)
- Node.js 22.12+ (Node 24 supported), npm
- Docker Engine and Docker Compose v2

## Cloud tool activation

This cloud instance has .NET installed under `/workspace/.dotnet`. In a fresh shell:

```bash
export DOTNET_ROOT=/workspace/.dotnet
export DOTNET_CLI_HOME=/workspace/.dotnet-cli
export NUGET_PACKAGES=/workspace/.nuget/packages
export npm_config_cache=/workspace/.npm
export PATH="$DOTNET_ROOT:$PATH"
```

These writable cache paths are specific to the managed cloud workspace; ordinary local
machines can use their default paths. No processes are assumed to survive environment restoration.

## Local setup

Run from the repository root:

```bash
cp .env.example .env
# Edit .env: choose a local password and keep ConnectionStrings__Platform in sync.
docker compose config --quiet
docker compose up -d --wait postgres
dotnet restore WhatsAppPlatform.slnx --locked-mode
dotnet build WhatsAppPlatform.slnx --no-restore
dotnet test WhatsAppPlatform.slnx --no-build
```

The .NET build also installs the client with `npm ci` and runs TypeScript/Vite checks.
NuGet and npm lockfiles are committed. No special global EF tool is needed; there are
no migrations or business entities yet.

To start the ASP.NET Core host with the built React assets:

```bash
set -a
source .env
set +a
dotnet run --project src/WhatsAppPlatform.Api --no-build --no-launch-profile
```

ASP.NET Core does not load `.env` automatically; Compose does. The host requires
`ConnectionStrings__Platform`. `/health/live` returns `{"status":"alive"}` and the
root serves the React shell. This is liveness, not database readiness. Unknown `/api/*`
routes return 404. No public business endpoints exist yet.

For interactive React development, keep the API running and in another terminal:

```bash
cd src/WhatsAppPlatform.Api/ClientApp
npm ci
npm run dev
```

Vite proxies API/health requests to port 5080. Standalone client validation:

```bash
cd src/WhatsAppPlatform.Api/ClientApp
npm run typecheck
npm run build
```

For a deployable combined artifact:

```bash
dotnet publish src/WhatsAppPlatform.Api -c Release -o ./artifacts/api
```

Configure the connection string, allowed hosts, and HTTPS termination for deployment.
The bootstrap is not an authenticated production product; business endpoints require
identity, authorization, tenant isolation, and secure operational configuration first.

## PostgreSQL

Compose starts PostgreSQL 17 on loopback with a persistent named volume and readiness
healthcheck. `database/init-schemas.sql` creates `organizations`, `whatsapp`, `messaging`,
`billing`, `identity`, and `platform` when the volume is first initialized. On an existing
volume, apply the idempotent schema script explicitly:

```bash
docker compose exec -T postgres sh -c 'psql -v ON_ERROR_STOP=1 -U "$POSTGRES_USER" -d "$POSTGRES_DB"' < database/init-schemas.sql
```

Check the actual database with:

```bash
docker compose exec -T postgres sh -c 'psql -v ON_ERROR_STOP=1 -U "$POSTGRES_USER" -d "$POSTGRES_DB" -c "SELECT schema_name FROM information_schema.schemata ORDER BY schema_name;"'
```

`docker compose down` stops services and retains data. Do not delete volumes casually.
The local owner account is for development only. Future production deployment must use
least-privilege roles, secure credentials, backups, and explicit migrations. EF is registered
scoped with Npgsql; registration and liveness do not open a database connection.

## Layout and test scope

- `src/WhatsAppPlatform.Domain`: framework-free contracts and future domain rules, grouped by context.
- `src/WhatsAppPlatform.Application`: future vertical use-case slices, depending only on Domain.
- `src/WhatsAppPlatform.Infrastructure`: EF Core/Npgsql and future external adapters.
- `src/WhatsAppPlatform.Api`: HTTP boundary, composition root, and `ClientApp` React UI.
- `tests/WhatsAppPlatform.Tests`: three deterministic unit smoke tests; no integration/E2E suites or coverage targets.

The smoke tests protect nonempty typed IDs, value equality without cross-context equality,
and scoped Npgsql context registration. They do not test framework behavior or properties
for coverage. See [architecture](docs/architecture.md) for dependencies and schema ownership,
and [domain model](docs/domain-model.md) for terminology and the two billing perspectives.
