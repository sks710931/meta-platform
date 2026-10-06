# WhatsApp Accounts onboarding implementation

Historical slice report; the current authentication and authorization rules are in [authentication.md](authentication.md).

This iteration adds domain/persistence scaffolding and local state views only. No Meta HTTP
calls, embedded browser signup, tokens, credit-line assignment, billing, sending, or webhooks
are implemented. Organization stays unchanged as an aggregate; it has no account collections.

## Final domain model

| Model | Fields and ownership |
| --- | --- |
| WhatsAppAccount | Internal WhatsAppAccountId, OrganizationId, SignupSessionId, typed ExternalWhatsAppAccountId, DisplayName, Status, CreatedAt, ConnectedAt |
| MessagingAccount | Internal MessagingAccountId, WhatsAppAccountId, typed ExternalMessagingAccountId, CreatedAt; minimal metadata owned by WhatsApp Accounts |
| PhoneNumber | Internal PhoneNumberId, WhatsAppAccountId, typed ExternalPhoneNumberId, DisplayPhoneNumber, optional VerifiedName, local Registered status, CreatedAt |
| EmbeddedSignupSession | Internal EmbeddedSignupSessionId, OrganizationId, Status, StartedAt, ExpiresAt, optional CompletedAt; independent aggregate |

MessagingAccountId is owned by WhatsAppAccounts.Contracts;
no Messaging implementation was introduced. All primary keys are nonempty typed UUIDs.
External IDs are separate opaque value objects, at most 100 Unicode characters,
with valid Unicode, required non-whitespace content, and no control characters,
never internal keys. Display names normalize whitespace (200-character limit); missing
account names fall back to the external ID. Phone displays are trimmed/nonempty (limit 50),
and optional verified names are trimmed (limit 200). All timestamps are UTC.

Account creation produces Connected records only from the registered result. Other suggested
account statuses are represented without adding transitions. Local Registered/Connected
states describe simulated onboarding and do not prove a real Meta connection or verification.

## Lifecycle

```text
Organization ID → start session → Pending
  → simulated external success → register result
  → account + messaging metadata + phone records created
  → Session Completed (all changes committed atomically)
```

Pending can transition to Completed, Failed, or Expired. Terminal states cannot reopen.
Completion must occur in [StartedAt, ExpiresAt), with UTC time. CompletedAt exists only for
Completed. Failed and expired attempts cannot complete; expiration is persisted lazily on
reads/completion attempts. Failure is a domain operation, not another public endpoint.
Lifetime defaults to 15 minutes and is configurable from 1 to 1440 minutes.

Repeated identical results return the same internal account without a second domain
transition. Reordered phones and normalized display whitespace remain equivalent. Different
results for the same completed session return 409. Conflicting external IDs across sessions
or organizations return 409 and roll back all attempted records, leaving the session Pending.
Session Status is an optimistic concurrency token. Unique indexes and EF's SaveChanges
transaction protect concurrent processing, not only preflight application checks.

## Tables, keys, constraints, and indexes

All four tables are in the existing `whatsapp` schema. Internal keys/FKs use uuid;
timestamps use timestamp with time zone; names/statuses/external IDs use bounded varchar.

| Table | Integrity and indexes added |
| --- | --- |
| accounts | UUID PK; unique external_whatsapp_account_id; unique signup_session_id; Organization FK; composite (signup_session_id, organization_id) FK enforcing the session's tenant; status/opaque-ID/nonempty-name/Connected-time checks; organization + descending created_at/id list index; composite session FK index |
| messaging_accounts | UUID PK; unique external_messaging_account_id; unique whatsapp_account_id (one messaging record per account); account FK; opaque external-ID check |
| phone_numbers | UUID PK; unique external_phone_number_id; account FK and FK index; opaque external-ID, nonempty display, and Registered-status checks |
| embedded_signup_sessions | UUID PK; alternate unique (id, organization_id) key; Organization FK; valid status, expiry-after-start, and completion-time/state checks; organization/start index; status optimistic concurrency token |

All FKs restrict deletion. External-ID uniqueness is global within each table for this
iteration. Transfer/reconnection semantics will require an explicit future design. The shared
DbContext supplies Organization FK composition without modifying the Organization aggregate.
There is one migration stream and one model snapshot; the existing snapshot path is retained.
The generated AddWhatsAppOnboarding migration supports both fresh install and upgrade from
the Organization migration. No migration is applied automatically by the HTTP host.

## API endpoints

| Method | Route | Semantics |
| --- | --- | --- |
| POST | /api/organizations/{organizationId}/whatsapp/onboarding-sessions | 201 session plus Location; 404 missing organization |
| GET | /api/whatsapp/onboarding-sessions/{sessionId} | 200 current session state; 404 missing |
| POST | /api/whatsapp/onboarding-sessions/{sessionId}/complete | **Temporary Development-only scaffolding**: 201 graph plus account Location; 200 identical replay; 400 invalid result; 404 missing; 409 terminal/conflicting registration; 410 expired |
| GET | /api/organizations/{organizationId}/whatsapp-accounts | 200 organization-filtered newest-first graph list; 404 missing organization |
| GET | /api/whatsapp-accounts/{whatsAppAccountId} | 200 clean account DTO with messaging metadata/phones; 404 missing |

Malformed/empty UUIDs return 400. Unknown completion fields are rejected rather than silently
accepting credentials. List/detail DTOs explicitly declare their fields and contain no tokens.
Session HTTP responses add manualCompletionAvailable based on the running environment;
outside Development it is false and the completion route returns 404.

Example **simulation** input (Development only):

```json
{
  "externalWhatsAppAccountId": "1001",
  "externalMessagingAccountId": "2001",
  "displayName": "Example account",
  "phoneNumbers": [
    { "externalPhoneNumberId": "3001", "displayPhoneNumber": "+1 555 0100", "verifiedName": "Example" }
  ]
}
```

The backend accepts 1–100 distinct phone entries. The minimal manual UI form records one
phone per simulated signup; account views display every persisted phone. Real Embedded
Signup is intentionally absent. Replace this HTTP scaffolding with a trusted external
signup adapter later while retaining the use case's validation, transaction, and replay rules.
Other endpoints remain unauthenticated within the existing scope; globally addressed detail
routes do not provide ownership authorization. Do not expose this scaffold to untrusted users.

## Exact tests added

Ten domain test methods add eleven unit cases (expiration has two cases):

| Test | Why valuable |
| --- | --- |
| New_session_is_pending_without_completion_time | Protects initial lifecycle state and completion-time invariant |
| Pending_session_completes_with_a_utc_timestamp | Proves valid completion changes status/time together |
| Completed_session_cannot_complete_again_or_rewrite_completion_time | Protects terminal state and original completion time |
| Failed_session_cannot_complete | Prevents reopening failed attempts |
| Session_cannot_complete_at_or_after_expiration | Two cases cover the exact expiry boundary and a later attempt |
| Invalid_completion_time_does_not_change_session_state | Rejects non-UTC or pre-start completion without mutation |
| Connected_account_has_internal_identity_ownership_and_utc_connection_time | Protects internal identity/tenant ownership, initial Connected state, normalized/fallback name, UTC connection time |
| Invalid_account_name_or_non_utc_time_is_rejected | Prevents invalid account records reaching persistence |
| External_identifiers_preserve_opaque_provider_values / External_identifiers_reject_missing_oversized_control_or_invalid_unicode_values | Protects the distinction and validation of provider identifiers |

The existing Internal_ids_reject_empty_identifiers test also covers EmbeddedSignupSessionId.
All 22 unit cases passed, none skipped. No coverage target or mocked suite was added.

The standalone Python smoke script has been removed during architecture cleanup.
Its valuable persistence and concurrency cases are deferred to
[future integration scenarios](future-integration-tests.md); no replacement test framework
or mocked persistence tests were added.

## Validation and scope report

- .NET solution builds with zero warnings/errors; all existing and new unit cases pass.
- React strict TypeScript and production build pass. No any or unchecked casts were introduced.
- Migration generated and applied to fresh databases and upgraded the normal development database; repeated application is up to date, pending-model check passes, and idempotent SQL reapplication succeeds.
- During the original slice validation, manual checks confirmed lazy expiry/410, failure/409, null completion times for terminal failures, and Production completion-route exclusion.
- Organization details contains the minimal account/phone list, Connect action, session state/refresh, and backend-gated manual form. Browser interaction was not automated.

No scope deviations. Extra SignupSessionId and its unique/composite keys support replay and
ownership integrity. Existing Organization work remains intact alongside this
iteration. No further feature or real Meta integration was started.
