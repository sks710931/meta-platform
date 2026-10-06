# Future onboarding integration-test scenarios

These scenarios preserve the intent of the removed standalone Python smoke script.
They are deferred until a lightweight PostgreSQL integration-test mechanism is authorized;
no integration infrastructure, mocked persistence tests, or coverage targets are added now.
Use an isolated migrated PostgreSQL database and the application persistence boundary.

- Atomic onboarding persistence: completing a session persists the account, messaging account,
  all phone numbers, and Completed state together.
- Duplicate external identifier rejection: each account, messaging, and phone external ID
  remains globally unique, including opaque nonnumeric provider values.
- Rollback on conflicts: any uniqueness conflict leaves no partial account graph and does
  not complete the losing session.
- Tenant-scoped account listing: an Organization sees only its own accounts and phones.
- Idempotent callback replay: an identical callback, including normalized display names and
  reordered phone records, returns the existing graph without inserting duplicates.
- Conflicting callback rejection: a different result for a completed session returns a
  conflict and preserves the original account graph and completion time.
- Concurrent duplicate callback handling: simultaneous identical callbacks converge on one
  account graph; the winner creates it and replays return that same persisted identity.

Also validate Domain and PostgreSQL identifier constraints against the same representative
Unicode, whitespace, control-character, and length-boundary fixtures when this suite exists.

## Future security integration scenarios

These require real HTTP cookies/antiforgery and PostgreSQL; do not substitute framework mocks.

- Anonymous calls to every business endpoint return 401; create requires PlatformAdmin.
- Secure/HttpOnly/SameSite and __Host cookie behavior behind the intended TLS/proxy topology.
- Login, logout, and each unsafe business method reject missing/invalid/cross-user CSRF tokens;
  safe GETs work without a request token; token refresh after login/logout is valid.
- Missing user, incorrect password, and locked-out login have the same generic error;
  lockout persists in PostgreSQL and process/edge rate limiting follows deployment policy.
- Organization catalog queries and /auth/me include only the current user's memberships;
  PlatformAdmin receives platform scope without fake membership.
- Direct organization and indirect account/session routes prevent cross-tenant enumeration;
  unauthorized session reads must not trigger lazy-expiry writes.
- Member reads succeed but onboarding administration/completion is forbidden; OrgAdmin is
  limited to its own tenant; Development completion is authenticated and absent in Production.
- Database rejects duplicate organization/user relationships and orphan membership FKs;
  accidental user/organization/role deletion is restricted.
- Idempotent bootstrap, incomplete-config failure, no default user in Production, and no
  password/membership-role changes from replayed development fixture provisioning.
- Absolute session deadline, security-stamp/password/role revocation, and membership removal
  take effect as documented; logout removes cookies and expired sessions return the UI to login.
- No hashes, stamps, password/token/cookie values appear in responses or diagnostic logs.

## Real Meta signup/security scenarios

Use an approved Meta test app/customer and isolated HTTP/PostgreSQL deployment, not SDK mocks.

- Confirm current v4 configuration, allowed HTTPS origins, user cancellation/final-screen closure,
  code TTL, actual business-token scope/expiry shape, and phased Messaging Account rollout.
- Tampered browser fields are rejected; code from the wrong app, insufficient scopes,
  unrestricted/multiple targets, and inaccessible account/phone resources fail closed.
- Real/meta and Development completion require authentication, stored tenant ownership, admin
  role, and CSRF; cross-tenant session IDs reveal no existence or credentials.
- Encrypted storage survives restart/replicas with the protected key ring; session-swapped
  ciphertext fails; responses, SDK console, HTTP client logs, proxies, and APM reveal no secrets.
- Concurrent submissions exchange only once; lost responses replay server state; crashes before
  and after reservation/exchange/checkpoint/discovery have the documented restart/retry behavior.
- Account graph conflicts roll back registration while retaining the recoverable credential;
  no duplicate credential/session, messaging, account, or phone identity is inserted.
- Real accounts with absent WAAC identifiers round-trip through EF and remain tenant-owned;
  existing manual/legacy records are not falsely interpreted as verified WAAC identities.
