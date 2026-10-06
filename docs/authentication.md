# Authentication and tenant authorization

This is a cross-cutting security slice across Identity, Organizations listing, and the
HTTP boundary of existing WhatsApp Accounts use cases. No Meta integration is implemented.

## Architecture

ASP.NET Core Identity stores Guid framework users/roles in Infrastructure. Domain exposes
UserId and OrganizationMembershipId as strongly typed nonempty UUID contracts; membership
is an independent small entity with OrganizationId, UserId, Role, and UTC CreatedAt.
Organization contains no users or memberships. Invalid membership roles and non-UTC times
are rejected in Domain; duplicate (OrganizationId, UserId) relationships are rejected by
PostgreSQL, rather than inventing a domain collection spanning all memberships.

Application owns ICurrentUser, IMembershipStore, IOrganizationAccessService, and the small
OrganizationAccessService/OrganizationAccessDecision. HTTP principal extraction is in API.
An API composition adapter bridges Identity SignInManager/UserManager to cookie operations;
Identity framework types never enter Domain, Application, or business endpoints.
ITenantResourceOwnership exposes only server-resolved OrganizationId for indirect resources.
Infrastructure owns its cross-cutting relational ownership reads and membership/name join.

```text
Authentication → Current User → PlatformAdmin?
                                  ├─ Yes → platform-wide access
                                  └─ No → OrganizationMembership → organization-scoped access
```

- PlatformAdmin is an Identity role, never a fake Organization membership. It creates
  organizations and reads/administers every organization.
- OrganizationAdmin is an organization-specific membership role: read access and permission
  to start onboarding or use the Development-only manual completion route in that organization.
- Member can read their organization's details, account information, and session state.
  Members cannot create organizations, start onboarding, or complete it manually.
- Membership checks query the current user's relationship on each request. Organization
  listing applies authorized organization IDs inside its database query. Admin gets all rows;
  an anonymous application context gets an empty scope, never an unrestricted scope.
- All tenant endpoint checks run in a reusable API filter using Application access decisions,
  before handlers execute (including the session lookup handler's lazy expiry update).

## Cookies, CSRF, and login security

Identity uses a same-origin application session cookie; there are no JWTs. It is HttpOnly,
SameSite=Strict, Path=/, nonpersistent, and Secure outside Development. Outside Development
its name is __Host-platform-auth, with no Domain attribute. Development allows loopback HTTP
(platform-auth, Secure when HTTPS). Sessions have a 30-minute absolute deadline, no sliding
expiration; the protected ticket retains that deadline across security-stamp regeneration.
Security stamps/platform roles are revalidated after five minutes; tenant membership is read
fresh. API authentication failures are 401/403, with no redirects or returnUrl handling.

GET /api/auth/csrf returns only an antiforgery request token and sets an HttpOnly same-site
antiforgery cookie (Secure and __Host-prefixed outside Development). Both cookie paths are /.
The React request helper fetches a fresh request token before unsafe requests and sends it in
X-CSRF-TOKEN. Authentication cookies remain unreadable to JavaScript. The token is also
required for login to prevent login CSRF, and is reacquired after authentication changes.
All API POST/PUT/PATCH/DELETE and other unsafe methods validate antiforgery; safe GET/HEAD/
OPTIONS requests do not. Missing/invalid tokens produce generic 400 responses. Authorization
middleware rejects unauthenticated protected endpoints with 401 before mutation/CSRF work.
API responses are no-store. No permissive CORS configuration is enabled.

Identity passwords require 12 characters and its standard uppercase/lowercase/digit/symbol
rules. Passwords use Identity's hasher. Five failed attempts lock a user out for 15 minutes.
Login additionally allows ten requests per remote IP per minute, with no queue (429 when
limited). Unknown users execute a dummy password verification to reduce timing enumeration;
unknown, incorrect-password, and locked-out attempts share the same generic 401 message.
Login credential lengths are bounded and unknown JSON fields are rejected. Accounts with
TwoFactorEnabled fail closed because MFA is not implemented. Passwords, cookies, antiforgery tokens,
hashes, and security stamps are never included in responses or application logs.
Framework Identity methods do not accept CancellationToken; adapters accept it and check
cancellation before those calls. Database/application methods propagate it normally.

## API authorization matrix

All existing organization and WhatsApp endpoints require authentication (anonymous → 401).
Unauthorized tenant resources return 404 whether absent or owned by another tenant,
preventing enumeration. A known member lacking administration privileges receives 403.
Malformed identifiers remain 400 after authentication; ownership is never trusted from a
client payload. PlatformAdmin bypasses membership, but missing resources still return 404.

| Endpoint | Authorization |
| --- | --- |
| POST /api/organizations | PlatformAdmin only; other authenticated users → 403; CSRF required |
| GET /api/organizations | PlatformAdmin sees all; others see only member organizations |
| GET /api/organizations/{organizationId} | Organization member or PlatformAdmin; otherwise 404 |
| POST /api/organizations/{organizationId}/whatsapp/onboarding-sessions | OrganizationAdmin or PlatformAdmin; CSRF required |
| GET /api/organizations/{organizationId}/whatsapp-accounts | Organization member or PlatformAdmin; otherwise 404 |
| GET /api/whatsapp/onboarding-sessions/{sessionId} | Resolve stored session owner, then member/PlatformAdmin check; otherwise 404 |
| GET /api/whatsapp-accounts/{whatsAppAccountId} | Resolve stored account owner, then member/PlatformAdmin check; otherwise 404 |
| POST /api/whatsapp/onboarding-sessions/{sessionId}/complete | Development only; stored owner resolved; OrganizationAdmin/PlatformAdmin and CSRF required |
| POST /api/auth/login | Anonymous allowed, antiforgery + login limiter; generic 401 failure, 204 success |
| POST /api/auth/logout | Authenticated + antiforgery; 204 success |
| GET /api/auth/me | Authenticated; only userId, email, isPlatformAdmin, and member organization ID/name/role |
| GET /api/auth/csrf | Anonymous allowed, no-store, only request-token response |
| GET /health/live and SPA/static assets | Anonymous intentional; no tenant data |

The completion endpoint does not exist outside Development. Unknown API routes remain 404.
React reads /api/auth/me, keeps state only in memory, shows email/logout, and uses server
contracts for UX visibility. A 401 returns to login; 403 displays a permission error. UI
visibility never supplies proof of ownership or authorization.

## Provisioning and deployment

There is no registration or membership management API. Explicit BootstrapAdmin__Email and
BootstrapAdmin__Password configuration provisions the first PlatformAdmin in Development
or Production. With neither supplied nothing is seeded; partial configuration fails closed.
Existing credentials are never reset. Provisioning is idempotent when repeated sequentially.
Use secret injection; remove bootstrap configuration after intentional provisioning. Run the
initial provisioning on a single instance to avoid competing first-time seed operations.
No password or user is defaulted in source/appsettings. .env.example contains comments and
placeholders only; Docker Compose does not automatically export these variables to the host.

Development-only DevelopmentMember__Email, __Password, __OrganizationId, and __Role create
one explicit user/membership fixture. Role must be Member or OrganizationAdmin and the
organization must already exist. Existing passwords/membership roles are not overwritten.
The fixture method is invoked only by the Development composition path; no HTTP route exists.

Identity uses identity.users, roles, user_roles, user_claims, user_logins, user_tokens,
role_claims, and organization_memberships. Tables are framework-generated with explicit
names. Membership has UUID PKs, a unique organization/user pair, role check, lookup indexes,
and Restrict FKs to Organization and User. Framework relationship FKs are also Restrict;
normalized email, username, and role names have unique indexes. Timestamps use timestamptz.
Apply AddIdentityAndOrganizationMemberships through the existing migration workflow.

Outside Development configure DataProtection__KeyDirectory to a restricted persistent key
ring; startup fails without it. Share/protect it across replicas, encrypt the volume at rest,
and plan backups/key rotation. Application-level certificate/KMS encryption of key files is
deferred. HTTPS and HSTS are enabled outside Development. Configure actual TLS certificates
or reviewed trusted-proxy forwarding before deployment; no arbitrary forwarded headers are
trusted. The IP limiter is process-local and uses the direct peer address: a deployment must
add shared/edge limiting and a reviewed proxy/IP policy for multiple replicas or proxy users.

## Deferred security work

Future HTTP/PostgreSQL security scenarios are listed in future-integration-tests.md; no
integration infrastructure is added. Revocation of copied session cookies is limited by
security-stamp validation; logout removes the browser cookie, not a server-side ticket record.
Role revocation can take up to five minutes. MFA, reset, email verification, registration,
invitations, account/session administration, and distributed brute-force controls are deferred.
Future authorization must protect Meta credentials, WhatsApp operations, messaging, templates,
billing, and credit-line operations. No sensitive Meta credentials exist in this iteration.
