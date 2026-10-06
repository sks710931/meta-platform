# Domain model

This describes intended ownership and boundaries. Organization creation/reads and the WhatsApp Accounts onboarding model are implemented.
External Meta integration and other workflows remain future work.

| Term | Owner | Intended boundary and references |
| --- | --- | --- |
| Organization | Organizations | Customer/tenant root identified by `OrganizationId`; no collection of WhatsApp accounts |
| WhatsApp Account | WhatsApp Accounts | Small root identified by `WhatsAppAccountId`, referencing `OrganizationId`; separate external Meta WABA identifier |
| Phone Number | WhatsApp Accounts | Independently addressable root identified by `PhoneNumberId`, referencing its `WhatsAppAccountId` and tenant; separate Meta phone-number identifier |
| Messaging Account | WhatsApp Accounts | Minimal messaging/payment identity associated with a connected account; `MessagingAccountId`, `WhatsAppAccountId`, and `OrganizationId` references |
| Credit Line Assignment | Billing | Small root identified by `CreditLineAssignmentId`, referencing customer `MessagingAccountId` and `OrganizationId`; records platform/Solution Partner credit-line association |
| Platform user/membership | Identity | Identity user uses UserId; OrganizationMembership is an independent entity with typed references and an organization-specific role |
| Partner/platform configuration | Platform Administration | Future operator-owned settings for the platform or Solution Partner; no credentials in Domain |

A WhatsApp Account and Messaging Account are distinct concepts even when Meta data
connects them closely. A Messaging Account is not a platform login or an Organization.
A Phone Number is not a raw telephone string or an external Meta ID. Account ownership
and messaging/payment linkage belong to explicit use cases, not navigation collections
inside Organization. Large aggregate graphs would couple unrelated lifecycle operations
and tenant/account cardinality; keep roots small and use typed ID references.

Credit Line Assignment describes association of the shared partner line with a customer
messaging account. It does not make the external credit-line identifier an aggregate key,
or imply that an assignment is already accepted, active, or billable. Lifecycle states,
authorization rules, and Meta acceptance must be designed when that feature is authorized.
Platform Administration owns partner setup; Billing owns financial associations and effects.

## Two separate billing perspectives

**Meta billing** describes measured Meta usage and amounts owed to Meta under the shared
platform/Solution Partner credit line. Preserve provider references and source measurements
for reconciliation and attribution to the correct organization/messaging account.

**Customer billing** describes the platform's commercial agreement with each Organization:
pricing, markups, charges, invoices, payments, and receivables. A customer charge can differ
from Meta cost in amount, currency, timing, or grouping. Never treat the partner credit-line
statement as the customer's invoice or mutate usage to represent a customer price.

Both perspectives belong to Billing but remain separate records/models with explicit
reconciliation references. Future financial corrections append records rather than rewrite
audit history; money uses integer minor units plus currency. No financial calculation,
invoice model, or credit-line operation is implemented here.

## Tenant and external-reference rules

- Tenant-owned roots carry `OrganizationId`. Every future tenant query filters by it and has a critical isolation test.
- Cross-context references use explicit ID contracts; they do not expose another root's mutable internals.
- Store internal primary keys as typed GUIDs/`uuid`. External Meta identifiers are separate provider references, not GUID substitutes or primary keys.
- Keep Graph API payloads, tokens, transport errors, and provider-specific enums in Infrastructure adapters. Translate them at the boundary.
- Persist UTC timestamps via `DateTimeOffset`; inject a clock for temporal rules.
- Organization create/list/details and local WhatsApp Account onboarding records are implemented. Identity authentication and organization authorization are implemented. No external Meta operations, updates/deletion, credit-line assignment, billing, sending, or webhooks are implemented.

## Implemented Organization aggregate

Organization has exactly `Id` (`OrganizationId`), `Name`, `Status`, and `CreatedAt`.
All properties are read-only. `Organization.Create` receives the ID and UTC timestamp;
it has no persistence, HTTP, random-ID generation, or system-clock dependency.

Names are case-preserving, with surrounding whitespace removed and whitespace runs
collapsed to one space. Empty/whitespace names, null characters, and normalized names
longer than 200 characters return validation failures. Duplicate names are allowed.
New roots always start Active; Suspended is a defined state, with no transition API yet.
Nonzero timestamp offsets are rejected rather than silently reinterpreted as UTC.
The application clock uses UTC millisecond precision; PostgreSQL stores `timestamptz`.
The aggregate contains no external identifiers, memberships, or connected-account collections.

## WhatsApp Accounts onboarding model

- **WhatsAppAccount**: typed internal Id, OrganizationId, SignupSessionId, typed external WABA ID, normalized DisplayName, Status, CreatedAt, ConnectedAt. New successful registrations are Connected; Pending/Suspended/Disconnected are defined but no transitions are exposed. SignupSessionId is an integrity/replay link, not a large aggregate graph.
- **MessagingAccount**: internal MessagingAccountId, WhatsAppAccountId, typed external messaging/payment ID, CreatedAt. This metadata entity is owned by WhatsApp Accounts, not the future message-sending context. Its internal ID contract is owned by WhatsAppAccounts.Contracts; Messaging remains reserved for future conversations, messages, outbound messages, and delivery status.
- **PhoneNumber**: internal PhoneNumberId, WhatsAppAccountId, typed external phone ID, trimmed DisplayPhoneNumber, optional trimmed VerifiedName, local Registered status, CreatedAt. Registered means locally recorded; it does not assert Meta provisioning/verification.
- **EmbeddedSignupSession**: typed internal Id, OrganizationId, Status, StartedAt, ExpiresAt, optional CompletedAt. It is an independent aggregate. There are no tokens, authorization codes, secrets, or Organization navigation collections.

All internal IDs remain UUIDs. External identifiers are distinct value objects containing
opaque, non-whitespace values of at most 100 Unicode characters, with valid Unicode and no control characters. They are never aggregate
primary keys. Account names preserve case, normalize whitespace, and have a 200-character
limit; absent names fall back to the external WABA ID. Phone displays are nonempty with a
50-character limit; verified names are optional with a 200-character limit. Timestamps are UTC.

```text
Organization (ID reference only)
    ↓ Start Embedded Signup Session
Pending
    ↓ External onboarding succeeds (currently simulated locally)
Register account result
    ↓ WhatsAppAccount created
    ↓ MessagingAccount created
    ↓ PhoneNumber(s) created
    ↓ Session Completed — one atomic database commit
```

Session transitions are Pending → Completed, Pending → Failed, or Pending → Expired.
Terminal states never reopen. CompletedAt exists only for Completed and falls in
[StartedAt, ExpiresAt). At the expiration boundary, completion is rejected. Expiration is
persisted lazily on state reads or completion attempts; there is no background worker.
Failure is a domain operation for future orchestration, not a public HTTP endpoint.

A repeated identical successful callback returns the existing internal account; it does not
repeat the domain transition. Different data for that session returns a conflict. Global
external-ID uniqueness prevents reconnecting the same external entity to another root/tenant
in this iteration. Transfer/reconnection policies and real Meta identifier rules must be
reviewed when the real integration is designed.

## Identity and organization authorization

UserId and OrganizationMembershipId are internal nonempty typed UUIDs. ASP.NET Core Identity
users/passwords/roles remain framework persistence concerns in Infrastructure. Domain has no
Identity framework types, credentials, HTTP principals, or authentication cookies.
OrganizationMembership contains Id, OrganizationId, UserId, Role, and UTC CreatedAt. It
validates defined roles and UTC time; PostgreSQL enforces unique (OrganizationId, UserId)
and Restrict FKs. Organization remains a separate aggregate with no membership collection.

PlatformAdmin is a platform Identity role granting cross-organization administration.
OrganizationAdmin grants administration of its own organization; Member grants read access.
Application's access service reads the current user's membership for the requested organization.
Authentication → Current User → PlatformAdmin? → platform access; otherwise membership →
organization-scoped access. Anonymous contexts never receive admin or unrestricted list scope.
See authentication.md for the endpoint matrix and deferred credential/operation protections.
