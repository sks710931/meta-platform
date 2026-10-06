# Embedded Signup v4 implementation

This slice adds real customer signup to the existing Organizations, Identity, and WhatsApp
Accounts boundaries. It is cross-cutting only to connect the React/API delivery adapters,
Application orchestration, and Infrastructure provider/credential persistence ports.
Domain contains no Meta HTTP, OAuth, SDK, or credential types.

## Configuration and deployment

Set all four environment/configuration values to enable Meta, or leave all four unset:
`Meta__AppId`, `Meta__AppSecret`, `Meta__EmbeddedSignupConfigurationId`, and
`Meta__GraphApiVersion`. Partial/malformed configuration fails startup without including
values in errors. App/config IDs and a version such as `v25.0` follow the official SDK
configuration format; use a currently supported version for your Meta app. There are no
committed secrets or default production credentials.

In Meta's App Dashboard create a Facebook Login for Business configuration selecting
**Embedded Signup v4 / WhatsApp Cloud API**, WhatsApp account assets, and only
`whatsapp_business_management` and `whatsapp_business_messaging`. Complete the applicable
Meta app review/Advanced Access requirements. Configure HTTPS hosting, allowed domains,
and valid OAuth redirect URIs in Meta before using signup. No unrelated product/permission
or future partner `business_management` credential is requested by the SDK wrapper.

Real signup requires `DataProtection__KeyDirectory` even in Development. Outside Development
also configure `DataProtection__CertificatePath` (private PFX) and its optional
`DataProtection__CertificatePassword` to encrypt newly persisted keys. Restrict access to
both the key directory and private certificate; protect existing keys and backups too.
All replicas must share the same persistent key ring, certificate access, and application
name. Certificate protection does not retroactively encrypt existing unprotected keys;
migrate/protect that storage before enabling production signup. Losing keys makes customer
credentials unusable. Neither key files nor certificates belong in source control.

Apply `AddProtectedMetaCredentials` using the existing EF commands before starting the host.
Do not downgrade after real connections with null WAAC IDs: the old required-column model
cannot represent them; no fake external ID is inserted to make rollback possible.

## Frontend and trust boundary

`ClientApp/src/meta/embeddedSignup.ts` contains all SDK globals/calls. It loads the official
SDK, initializes with public AppId/Graph version, and calls `FB.login` using configuration
ID, code response, override flag, and empty setup extras. v4 is chosen by the Dashboard
configuration, not an invented version parameter. SDK automatic app-event logging is disabled.

Connect creates the existing session. **Continue with Meta** then launches the prepared SDK
from a direct user gesture so browsers permit the popup. This extra click is deliberate.
The wrapper accepts session events only from exact official Facebook origins, ignores asset
hints/raw provider error strings, and waits for `authResponse.code` on successful login.
Final-screen closure can still succeed. Cancel, missing result/closed or blocked popup,
SDK load failure, and provider signup failure get distinct safe messages.

The browser submits only `{ authorizationCode }`; no OrganizationId, WABA, or phone ID is
accepted. The code stays in a local promise/request long enough to submit, never React state,
storage, URL, or logs. Meta documents a 30-second code TTL. Retry saved result submits null
and reuses a server checkpoint/completed graph; it never stores or resends the original code.
Successful completion refreshes session/account/phone state. Existing authentication expiry
handling returns to login. Development retains its explicit manual fallback.

## API and authorization

| Endpoint | Authorization and behavior |
| --- | --- |
| GET `/api/whatsapp/embedded-signup/configuration` | Authenticated; safe allowlisted public config only, no-store |
| POST `/api/whatsapp/onboarding-sessions/{sessionId}/meta-complete` | Authenticated, stored session ownership, OrganizationAdmin or PlatformAdmin, antiforgery |
| Existing session creation | Existing organization administration and antiforgery unchanged |
| Existing session/account/list reads | Existing membership/platform scope unchanged |
| Development `/complete` | Development-only, authenticated, organization administration and antiforgery unchanged |

Production completion rejects unknown JSON fields. Anonymous requests return 401, another
tenant's session returns 404, and an own-tenant Member gets 403. Malformed input is 400;
expired onboarding is 410; conflict/in-progress/restart-required is 409; unsupported or denied
provider resources are 422; disabled/unavailable provider is 503. Creation is 201 and persisted
completion replay is 200. Responses contain the existing safe account/session DTOs only.

## Graph calls and authoritative discovery

All calls use HttpClientFactory, a fixed HTTPS Graph host, configured version, 20-second
request timeout, a bounded JSON response, disabled redirects/cookies, cancellation, and no
automatic retries. HttpClientFactory logging is removed for this client because exchange and
debug-token URLs contain secrets. Do not enable outbound URL/header/body tracing for Meta
in deployment APM/proxies. Provider response bodies/exceptions are not returned or logged.

| Graph endpoint | Server use |
| --- | --- |
| GET `/{version}/oauth/access_token` | Exchange code with AppId/AppSecret exactly once |
| GET `/{version}/debug_token` | Inspect returned customer token using server-only app authorization |
| GET `/{version}/{authorized-waba-id}?fields=id,name` | Verify node identity and discover display name |
| GET `/{version}/{authorized-waba-id}/phone_numbers?fields=id,display_phone_number,verified_name&limit=100` | Discover associated phones; controlled cursor pagination |

Debug verification requires correct AppId, valid token, both WhatsApp permissions, no known
expired token/data-access deadline, and **one explicit management target**. Unrestricted or
multiple targets fail closed; a browser hint cannot choose a resource. Node ID must match
that verified target; phone association comes from its server-side edge. The existing domain
validation, 1–100 distinct-phone bound, and database constraints remain the final boundary.
No browser-supplied display or identity field enters registration.

The returned credential is documented as a **business integration system user access token**
(customer business token), not the future platform Solution Partner/system-user token.
No credential expiry is invented or stored; known debug deadlines are checked on discovery.
The unavailable separate WAAC identifier is left null following the user-approved adjustment.
See [official contract review](meta-embedded-signup-contract-review.md) for sources and the
beta/phased-account-model caveat. No WAAC endpoint/identifier is guessed.

## Credential persistence, concurrency, and replay

`whatsapp.meta_credentials` stores internal UUID, unique signup_session_id, nullable
protected_credential, and UTC CreatedAt/UpdatedAt. The session FK is Restrict. Checks enforce
nonempty ciphertext when present and ordered timestamps. Null ciphertext means an exchange
reservation; no code or plaintext token is persisted. The account external WAAC column becomes
nullable; its unique index and existing external Messaging Account/phone uniqueness remain.

A dedicated unpooled PostgreSQL connection holds a session advisory lock for the entire attempt.
Concurrent submissions receive 409 and cannot exchange simultaneously. A reservation is
committed **before** exchange. On successful exchange the token is immediately protected with
Data Protection purpose `WhatsAppPlatform.Meta.CustomerCredential.v1` plus the session UUID,
and committed before discovery. This binds ciphertext to the session. The store implements
an Application port so a future vault can replace it without moving secrets into Domain.

Meta and PostgreSQL cannot share a transaction. A failed/uncertain exchange or interrupted
checkpoint leaves a marker requiring a fresh signup; even a crash cannot justify blindly
re-exchanging that session's one-time code. A saved credential permits discovery retry.
Once completed, replay returns persisted server state without calling Meta or creating new
credentials/accounts/phones. The existing registration handler commits the account graph and
Completed state atomically; external uniqueness and session concurrency remain authoritative.

Development manual completion and real signup should not be mixed for the same attempt.
Orphaned/expired encrypted credential retention, revocation, automated cleanup, rotation,
and eventual vault migration are deferred. No credential read API is exposed.

## Verification and focused tests

.NET build passes with zero warnings/errors; all **37 unit cases** pass. React standalone
TypeScript check and production build pass. The full migration chain applies to a disposable
PostgreSQL database; EF reports no pending model changes. The disposable database is removed.
Source review confirms no secret fields in Domain/public config/response DTOs, no raw provider
logging, ciphertext-only credential assignments, and authenticated tenant/admin/CSRF enforcement
on real and Development completion. An ephemeral Development host against a second disposable
migrated database confirmed anonymous completion 401, cross-tenant completion 404, missing CSRF
400 on both completion routes, unknown production-body fields 400, disabled Meta 503, and safe
configuration fields. Existing bootstrap/fixture provisioning was used; no test infrastructure
was committed. No real Meta secrets were available for a live signup.

Added tests (eight cases across five methods):

- `Discovery_uses_one_explicitly_authorized_account_and_fails_closed`: wrong app, invalid token,
  missing permissions, missing/multiple asset scope, malformed/expired deadlines cannot map.
- `Credential_checkpoint_prevents_reexchange_on_replay_or_uncertain_exchange`: fresh exchange,
  stored credential reuse, and restart after an uncertain exchange are distinct.
- `Credential_cannot_leak_through_serialization_or_string_formatting`: secret-bearing contract
  produces neither token JSON nor token string output, and MetaOptions excludes AppSecret.
- `Provider_errors_are_classified_without_exposing_provider_content` (four cases): safe rejection
  versus temporary-unavailability classification, without raw Graph messages.
- `Local_connection_can_defer_provider_waac_identity_without_losing_ownership`: approved absent
  WAAC identity retains tenant ownership and still requires a meaningful display name.

No framework/SDK/HttpClient tests, mocked persistence suites, integration infrastructure,
or coverage targets are added. Future real Meta/HTTP/PostgreSQL scenarios are recorded in
[future integration scenarios](future-integration-tests.md).

## Limits and deviations

The user approved the nullable external WAAC model adjustment after reviewing official docs.
The popup uses an additional direct-gesture click. Broad or multi-target customer grants are
rejected rather than guessed; actual token scope shapes/app rollout need sandbox verification.
No live Meta exchange/signup was run here. Connected/Registered remain local persistence
states: phone registration, webhook subscription, payment setup, credit-line assignment,
billing, sending, templates, and conversations are outside this slice.
