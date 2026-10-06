# WhatsApp Platform

A modular SaaS foundation for managing multiple WhatsApp Business accounts and attributing
Meta billing under a shared partner credit line. Organizations supports create, list, and details.
WhatsApp Accounts supports local onboarding sessions, account records, and real Meta Embedded Signup v4.

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
dotnet tool restore
dotnet restore WhatsAppPlatform.slnx --locked-mode
dotnet build WhatsAppPlatform.slnx --no-restore
set -a
source .env
set +a
dotnet ef database update --project src/WhatsAppPlatform.Infrastructure --no-build
dotnet test WhatsAppPlatform.slnx --no-build
```

The .NET build also installs the client with `npm ci` and runs TypeScript/Vite checks.
NuGet/npm lockfiles and the local EF tool manifest pin dependency and tooling versions. Migrations are applied
explicitly; the host does not migrate or create its database on startup.

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
routes return 404. The Organizations API and React views are available.

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
All business APIs require Identity cookie authentication and organization authorization.
Read [authentication setup](docs/authentication.md) before starting: provision an explicit
PlatformAdmin using injected configuration; no default credentials or public registration exist.

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
scoped with Npgsql; Organizations requests use the database, while liveness does not.

## Layout and test scope

- `src/WhatsAppPlatform.Domain`: framework-free contracts and future domain rules, grouped by context.
- `src/WhatsAppPlatform.Application`: future vertical use-case slices, depending only on Domain.
- `src/WhatsAppPlatform.Infrastructure`: EF Core/Npgsql and future external adapters.
- `src/WhatsAppPlatform.Api`: HTTP boundary, composition root, and `ClientApp` React UI.
- `tests/WhatsAppPlatform.Tests`: 29 deterministic unit test cases; no integration/E2E suites or coverage targets.

The tests protect nonempty typed IDs, value/type identity, scoped Npgsql registration,
and critical Organization creation invariants. They do not test framework behavior or properties
for coverage. See [architecture](docs/architecture.md) for dependencies and schema ownership,
and [domain model](docs/domain-model.md) for terminology and the two billing perspectives.

## Organizations API

| Method | Route | Outcome |
| --- | --- | --- |
| POST | `/api/organizations` | Body `{ "name": "Example" }`; 201 with Location and created organization, or 400 validation errors |
| GET | `/api/organizations` | 200 array ordered by CreatedAt descending, then internal ID descending |
| GET | `/api/organizations/{organizationId}` | 200 details; 404 unknown ID; 400 malformed/empty UUID |

All responses contain `organizationId`, `name`, `status` (`Active` or `Suspended`), and
`createdAt` (UTC ISO timestamp). Names normalize whitespace and have a 200-character
normalized limit. There are no update/delete endpoints. The UI starts at the list,
opens a create form, and shows details after selection or successful creation.

To review or generate deployment SQL without exposing credentials:

```bash
dotnet ef migrations has-pending-model-changes --project src/WhatsAppPlatform.Infrastructure
dotnet ef migrations script --idempotent --project src/WhatsAppPlatform.Infrastructure --output /tmp/organizations.sql
```

EF uses the same `ConnectionStrings__Platform` environment variable as the host. The
first migration creates `organizations.organizations`, with UUID key, bounded name/status,
UTC timestamp, check constraints, and a descending list-order index. History lives in
`platform`. Apply migrations with a privileged deployment role, then use a least-privilege
runtime role. Do not apply migrations automatically inside an HTTP request.

## WhatsApp Account onboarding (local scaffolding)

Organization details now shows account state, connected date, phones, and a Connect WhatsApp
Account action. It starts a session; a manual completion form appears only when the backend
runs in Development. This is a simulation, not real Meta signup or verification.

| Method | Route | Result |
| --- | --- | --- |
| POST | `/api/organizations/{organizationId}/whatsapp/onboarding-sessions` | 201 Pending session; 404 missing organization |
| GET | `/api/whatsapp/onboarding-sessions/{sessionId}` | 200 current session; 404 missing |
| POST | `/api/whatsapp/onboarding-sessions/{sessionId}/complete` | Development-only: 201 registered graph; 200 identical replay; 400 invalid data, 409 conflict, 410 expired |
| GET | `/api/organizations/{organizationId}/whatsapp-accounts` | 200 tenant-scoped accounts/phones; 404 missing organization |
| GET | `/api/whatsapp-accounts/{whatsAppAccountId}` | 200 account details; 404 missing |

Malformed/empty UUIDs are 400. Lifetime is configured with
`WhatsAppOnboarding__SessionLifetimeMinutes` (default 15, range 1–1440). Apply the new migration
using the existing EF commands before startup. For the lifecycle, schema constraints,
completion payload, and exact test list, see [onboarding report](docs/whatsapp-onboarding-implementation.md).

Testing remains limited to small deterministic unit tests run through `dotnet test`.
Persistence and concurrency checks are documented as [future integration scenarios](docs/future-integration-tests.md),
without introducing integration-test infrastructure. No credit-line assignment, billing, sending, or webhooks are implemented.

## Authentication setup

Apply the Identity migration before configuring bootstrap/startup. Inject BootstrapAdmin__Email
and BootstrapAdmin__Password from your local environment or production secret store, never
commit their values, and remove them after provisioning. Production also requires HTTPS and
DataProtection__KeyDirectory pointing to a protected persistent key ring. No default password
is created. See [authentication architecture and endpoint rules](docs/authentication.md).

For explicit local membership fixtures only, set DevelopmentMember__Email, __Password,
__OrganizationId, and __Role (Member or OrganizationAdmin). The organization must exist;
restart the Development host to provision the fixture idempotently. There is no membership API.

## Real Meta Embedded Signup v4

Apply the Meta credential migration and configure the four `Meta__*` values from secret/configuration
storage. Real signup requires persistent Data Protection keys; Production also requires key-encryption
certificate configuration. AppSecret never reaches React. The customer flow uses only the two WhatsApp
permissions and leaves partner credit-line credentials separate. Connect starts a session; Continue
with Meta opens the popup; Retry saved result recovers a persisted checkpoint without reusing a code.
See [deployment, API contracts, verification, and limitations](docs/meta-embedded-signup.md).
