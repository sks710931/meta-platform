# Organizations implementation report

Only create, list, and details are implemented. No update/delete, membership, identity,
authorization, Meta, WhatsApp, billing, or messaging functionality was added. The project
structure is preserved. Shared changes are limited to composition, migration tooling,
package alignment, and documentation necessary for this Organizations slice.

## Domain and behavior

Organization has four read-only properties: typed OrganizationId, normalized Name,
OrganizationStatus (Active/Suspended), and UTC DateTimeOffset CreatedAt. OrganizationId
was reused unchanged. Creation preserves case, trims/collapses whitespace, rejects
empty/whitespace/null-character names and normalized names over 200 characters, rejects
non-UTC timestamps, and always starts Active. Expected failures return explicit results.
The Application handler injects TimeProvider and creates timestamps at UTC millisecond
precision to preserve the same value across PostgreSQL response roundtrips.

The purpose-specific IOrganizationStore implements only add/list/find. Results become
application response DTOs rather than exposing tracked entities. List ordering is
CreatedAt descending then ID descending. Details returns explicit Found/NotFound results.

## HTTP and UI

- POST `/api/organizations`: Name input; 201 plus Location and organization response, or 400 validation errors.
- GET `/api/organizations`: 200 list, newest first with deterministic ID tie-break.
- GET `/api/organizations/{organizationId}`: 200 details; 404 missing; 400 invalid/empty UUID.
- Responses contain organizationId, name, status, createdAt. Malformed JSON/type mismatches return 400. Unexpected exceptions return generic 500 Problem Details without database details or stack traces.
- React has list/create/details views, typed runtime-checked contracts, loading/cancellation, validation and server errors, retry controls for reads, and creation success feedback.

## Persistence and migration

`organizations.organizations` stores id uuid, name varchar(200), status varchar(20), and
created_at timestamp with time zone. Explicit mapping provides check constraints and
an index on (created_at DESC, id DESC). The generated CreateOrganizations migration,
designer, and model snapshot are included as repository source files. A local pinned EF tool and
design-time context factory support explicit migrations using ConnectionStrings__Platform.
The relational EF package was pinned to 10.0.12 to resolve a tooling-related version conflict.

## Tests added

Four methods in OrganizationTests add eight cases (the three existing smoke tests remain):

| Test | Cases | Why it matters |
| --- | --- | --- |
| Creation_normalizes_name_and_starts_active_with_supplied_utc_time | 1 | Exercises valid creation, normalization, initial state, and explicit injected timestamp |
| Creation_rejects_invalid_names | 5 | Null, empty, spaces, tabs/newlines, and null-character input must fail before persistence |
| Creation_rejects_names_exceeding_storage_limit | 1 | Prevents valid-looking input failing at the database storage boundary |
| Creation_rejects_non_utc_timestamp | 1 | Protects persisted UTC time invariant |

No mocked test suite, integration/E2E framework, or coverage target was added. Manual
HTTP/PostgreSQL validation scripts were temporary local checks, not repository test suites.

## Validation performed

- .NET solution build: zero warnings/errors; all 11 tests passed, none skipped.
- React standalone TypeScript check and production build passed.
- EF generated the migration; it applied to a fresh validation database and the normal development database. Reapplying reported up to date. Pending-model-change check passed; idempotent deployment SQL generated and reapplied successfully.
- HTTP checks confirmed normalized create/201/Location, UTC and typed-ID serialization, persisted details roundtrip, newest-first ordering, deterministic tied-timestamp ordering, Active/Suspended mapping, 400 malformed/invalid input, and 404 missing details. Rejected requests added no records.
- A separate temporary host with a nonexistent database returned generic 500 Problem Details without infrastructure details.

## Scope decisions and limitations

No scope deviations. The 200-character name limit and null-character rejection make
storage validation explicit; duplicate names remain permitted. There is no pagination,
update/delete operation, status transition, or authentication. The global unauthenticated
catalog follows the requested scope and requires access control before untrusted deployment.
Browser interaction was not automated; the React source was strictly type-checked and built,
and the API/persistence behavior was exercised directly. No following feature was started.

## All added files

- `docs/organizations-implementation.md`
- `dotnet-tools.json`
- `src/WhatsAppPlatform.Api/ClientApp/src/organizations/CreateOrganization.tsx`
- `src/WhatsAppPlatform.Api/ClientApp/src/organizations/OrganizationDetails.tsx`
- `src/WhatsAppPlatform.Api/ClientApp/src/organizations/OrganizationsList.tsx`
- `src/WhatsAppPlatform.Api/ClientApp/src/organizations/organizationsApi.ts`
- `src/WhatsAppPlatform.Api/Organizations/CreateOrganizationEndpoint.cs`
- `src/WhatsAppPlatform.Api/Organizations/CreateOrganizationRequest.cs`
- `src/WhatsAppPlatform.Api/Organizations/GetOrganizationDetailsEndpoint.cs`
- `src/WhatsAppPlatform.Api/Organizations/ListOrganizationsEndpoint.cs`
- `src/WhatsAppPlatform.Application/Organizations/Contracts/IOrganizationStore.cs`
- `src/WhatsAppPlatform.Application/Organizations/Contracts/OrganizationResponse.cs`
- `src/WhatsAppPlatform.Application/Organizations/CreateOrganization/CreateOrganizationHandler.cs`
- `src/WhatsAppPlatform.Application/Organizations/CreateOrganization/CreateOrganizationResult.cs`
- `src/WhatsAppPlatform.Application/Organizations/GetOrganizationDetails/GetOrganizationDetailsHandler.cs`
- `src/WhatsAppPlatform.Application/Organizations/GetOrganizationDetails/GetOrganizationDetailsResult.cs`
- `src/WhatsAppPlatform.Application/Organizations/ListOrganizations/ListOrganizationsHandler.cs`
- `src/WhatsAppPlatform.Domain/Organizations/Organization.cs`
- `src/WhatsAppPlatform.Domain/Organizations/OrganizationCreationResult.cs`
- `src/WhatsAppPlatform.Domain/Organizations/OrganizationStatus.cs`
- `src/WhatsAppPlatform.Infrastructure/Organizations/EfOrganizationStore.cs`
- `src/WhatsAppPlatform.Infrastructure/Organizations/Migrations/20261006062606_CreateOrganizations.Designer.cs`
- `src/WhatsAppPlatform.Infrastructure/Organizations/Migrations/20261006062606_CreateOrganizations.cs`
- `src/WhatsAppPlatform.Infrastructure/Organizations/Migrations/PlatformDbContextModelSnapshot.cs`
- `src/WhatsAppPlatform.Infrastructure/Organizations/OrganizationConfiguration.cs`
- `src/WhatsAppPlatform.Infrastructure/Persistence/PlatformDbContextFactory.cs`
- `tests/WhatsAppPlatform.Tests/OrganizationTests.cs`

## All materially changed files

- `AGENTS.md`
- `README.md`
- `docs/architecture.md`
- `docs/domain-model.md`
- `src/WhatsAppPlatform.Api/ClientApp/src/App.tsx`
- `src/WhatsAppPlatform.Api/ClientApp/src/styles.css`
- `src/WhatsAppPlatform.Api/Program.cs`
- `src/WhatsAppPlatform.Api/packages.lock.json`
- `src/WhatsAppPlatform.Application/Organizations/README.md`
- `src/WhatsAppPlatform.Domain/Organizations/README.md`
- `src/WhatsAppPlatform.Infrastructure/DependencyInjection.cs`
- `src/WhatsAppPlatform.Infrastructure/WhatsAppPlatform.Infrastructure.csproj`
- `src/WhatsAppPlatform.Infrastructure/packages.lock.json`
- `tests/WhatsAppPlatform.Tests/packages.lock.json`
