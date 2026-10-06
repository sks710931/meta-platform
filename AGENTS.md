# Agent rules

Hard rules for every change. Product behavior lives elsewhere. These rules are about code quality only.

## Boundaries

- One module, one reason to change. Do not add a second responsibility to an existing module.
- Dependency direction is one way: delivery → application → domain. Domain imports nothing from delivery or infrastructure.
- Modules do not import each other's internals. Share a type only through an explicit contract.
- No business rule in a controller, handler, component, or migration.
- A change touches one module, or it is labeled cross-cutting and says why. Do not refactor a neighbor in the same change.

## Files

- One use case per file. Name the file after the action.
- A file over 200 lines must be split, unless it is generated.
- A function does one thing. If the name needs "and", split it.
- No global database, client, or config read inside domain code. Pass dependencies in.
- Public functions name their inputs. No boolean flags that switch behavior. Split the function.

## Types and errors

- Strict types. No `any`. No unchecked casts.
- Parse unknown input at the edge. Domain code receives typed values.
- Expected failures return a result. Throw only for programmer errors.
- Do not swallow errors. Do not catch and return null.

## Data

- Money is integer minor units. No floats for money.
- Time is an injected clock. Do not call the system clock in domain code.
- Writes that correct history are new records. Do not mutate an audit row.
- Every tenant-scoped query filters by tenant. Add a test when you add a query.

## Tests

- Critical domain rules have small, deterministic unit tests with no network, no filesystem, and no real clock.
- Test the module through its public contract. Do not mock the unit under test.
- A bug fix includes a failing test that the fix turns green.
- Do not assert on log text or private fields.

## Diffs

- No drive-by format, rename, or dependency upgrade.
- No commented-out code. No new TODO without an owner and a reason.
- Secrets, tokens, and local env files are never committed.
- Generated code is regenerated, not hand-edited.
- Before concluding a repository task, commit its changes and push the current branch. Preserve unrelated changes, never force push, and report any commit or push blocker.
- If a rule in this file conflicts with a one-off shortcut, the rule wins.

## Platform architecture (cross-cutting bootstrap)

- This repository is a modular monolith on .NET 10, ASP.NET Core, strict React TypeScript, and PostgreSQL/Npgsql.
- Read `docs/architecture.md` and `docs/domain-model.md` before adding a feature.
- Domain has no package dependencies. Application depends only on Domain.
- Infrastructure depends on Application/Domain. API references Infrastructure only as the composition root; HTTP slices depend on Application contracts.
- Keep Organizations, WhatsApp Accounts, Messaging, Billing, Identity, and Platform Administration separate. Import only explicit contracts across contexts.
- Add vertical slices inside the owning context when a feature is authorized. Do not invent generic repositories, services, or placeholder handlers.
- Aggregate identifiers are typed internal IDs. Meta identifiers are external references and never internal primary keys.
- Organization holds no collection of connected WhatsApp accounts. Tenant-owned aggregates reference `OrganizationId`.
- Explicitly map each future EF entity to its context-owned schema. Use one physical database initially.
- Persist UTC `DateTimeOffset` values as PostgreSQL `timestamp with time zone`. Inject clocks for domain behavior.
- Every async API accepts and propagates `CancellationToken`.
- Separate Meta usage/liability from customer pricing, charges, and receivables.
- React uses strict TypeScript. Do not relax compiler checks to make builds pass.
- Keep smoke tests intentionally small. Add tests for critical rules, security, billing, state transitions, and high-risk logic. No trivial property tests, coverage targets, large mocked suites, or integration/E2E suites in this iteration.
- Validate with `dotnet build WhatsAppPlatform.slnx`, `dotnet test WhatsAppPlatform.slnx --no-build`, client `npm run build`, and `docker compose --env-file .env.example config --quiet`.
- Organizations supports create/list/details. WhatsApp Accounts supports onboarding records and Development-only manual result registration. Do not expand to Organization update/delete, identity, Meta integration, credit-line assignment, billing, sending, or webhooks without authorization.
