# Authentication slice implementation report

Cross-cutting Identity/authentication and tenant-authorization slice. No additional product
work, Meta integration, billing, messaging, registration, or membership management API.
Read [authentication architecture](authentication.md) for the complete cookie/antiforgery,
provisioning, database, role, endpoint, and deployment configuration.

## Domain and boundaries

UserId and OrganizationMembershipId are strongly typed UUIDs. OrganizationMembership has
Id, OrganizationId, UserId, OrganizationRole, and UTC CreatedAt; roles are OrganizationAdmin
and Member. Membership is independent of Organization. Framework users/roles and credential
hashes remain in Infrastructure. Application owns reusable user/membership/access contracts;
API composition adapters handle Identity cookies and HTTP claims. No generic repositories or
permission engine were added. WhatsApp lifecycle/registration persistence code is unchanged.

## Exact tests added or changed

| Test | Value |
| --- | --- |
| Membership_binds_a_valid_role_to_one_user_and_organization_at_utc_time | Protects the security relationship's identity, tenant, defined role, and UTC time |
| Invalid_membership_role_or_non_utc_timestamp_is_rejected | Prevents malformed permission records entering persistence |
| Member_reads_own_organization_but_cannot_administer_or_access_another_tenant | Protects read/admin distinction and tenant isolation |
| Organization_admin_has_no_platform_wide_authority | Prevents confusing tenant administration with platform administration |
| Platform_admin_bypasses_membership_and_gets_unrestricted_catalog_scope | Confirms explicit platform-wide administration without fake memberships |
| Anonymous_user_is_denied_even_if_an_admin_flag_is_supplied | Prevents anonymous privilege escalation and unrestricted listing |
| Catalog_scope_contains_only_the_current_users_memberships | Protects current-user scope used by database organization listing |

The existing Internal_ids_reject_empty_identifiers test also checks UserId and
OrganizationMembershipId. All existing domain/lifecycle and context registration tests remain.
The access-service tests use one tiny deterministic application-port fixture; they do not
simulate or assert EF, database concurrency, password hashing, or cookie internals.
Duplicate membership prevention is a database uniqueness guarantee, not a unit test of a
fabricated aggregate collection. Future HTTP/PostgreSQL security scenarios are documented in
[future integration scenarios](future-integration-tests.md). No integration/E2E framework,
retained standalone smoke script, large mocked suite, or coverage target was added.

## Database and verification

Added identity.users, roles, user_roles, user_claims, user_logins, user_tokens, role_claims,
and organization_memberships. All eight relationship FKs are ON DELETE RESTRICT.
Membership's organization/user pair and Identity normalized email/username/role names are
unique. The new migration and shared snapshot were generated with EF tooling.

- Full .NET build passed with zero warnings/errors; all 29 unit cases passed, none skipped.
- Standalone React TypeScript and production build passed, and the host's integrated client
  build passed. Browser interaction was not automated.
- EF reports no pending model changes. Idempotent SQL applied to a fresh isolated PostgreSQL
  database and upgraded the normal local database; reapplication succeeded. Actual tables,
  unique indexes, and Restrict FKs were inspected.
- One-off HTTP verification with an isolated database confirmed every business endpoint is
  401 to anonymous callers; create is PlatformAdmin-only; tenant list/detail and indirect
  account/session lookups enforce ownership; read-only members get 403 for administration.
- Login/logout and onboarding writes reject missing CSRF tokens; safe reads do not require
  request tokens. Auth/me returns only safe fields. Identical onboarding completion still
  returns the existing graph. Explicit seed replay is idempotent.
- Invalid/unknown/locked-out logins share a generic 401 response; five failed attempts lock
  the account, and ten requests per direct remote IP/minute trigger 429.
- A locally trusted HTTPS Production host confirmed Secure/HttpOnly/SameSite=Strict auth
  cookies, platform access, and 404 for the absent manual completion endpoint. Development
  completion requires authentication, tenant administration, and antiforgery.

No unintentional anonymous organization/WhatsApp endpoints remain. Intentional anonymous
surfaces are login, CSRF token retrieval, liveness, and public SPA/static assets.

## Tradeoffs and scope

Direct organization resources and indirect tenant resources return 404 to nonmembers;
known members requesting administration receive 403. Start onboarding requires OrgAdmin
rather than read-only membership. Cookie sessions have a protected absolute 30-minute
limit; role/security-stamp validation is five minutes, membership is checked every request.
Production requires explicit persistent DataProtection key-directory configuration and TLS.
Key-ring encryption, trusted proxy configuration, multi-replica/edge rate limiting, copied
cookie revocation, and the intentionally excluded account-recovery/MFA flows are deployment
or future work described in authentication.md. Framework auxiliary sign-out schemes are
registered securely solely for Identity's internal stamp rejection; no external/MFA flow
exists and no auxiliary authentication cookie is issued by this slice.

No scope deviations. The small Development configuration fixture is the explicitly permitted
minimum mechanism for proving membership authorization; it has no public API and never runs
in Production. No default bootstrap credentials were added. Bootstrap configuration should
be removed after intentional provisioning. Existing passwords/membership roles are unchanged
on seed replay.

## Files added or materially changed

- `.env.example`
- `AGENTS.md`
- `README.md`
- `docs/architecture-cleanup.md`
- `docs/architecture.md`
- `docs/authentication-implementation.md`
- `docs/authentication.md`
- `docs/domain-model.md`
- `docs/future-integration-tests.md`
- `docs/organizations-implementation.md`
- `docs/whatsapp-onboarding-implementation.md`
- `src/WhatsAppPlatform.Api/ClientApp/src/App.tsx`
- `src/WhatsAppPlatform.Api/ClientApp/src/auth/AuthenticationShell.tsx`
- `src/WhatsAppPlatform.Api/ClientApp/src/auth/Login.tsx`
- `src/WhatsAppPlatform.Api/ClientApp/src/auth/apiFetch.ts`
- `src/WhatsAppPlatform.Api/ClientApp/src/auth/authApi.ts`
- `src/WhatsAppPlatform.Api/ClientApp/src/organizations/OrganizationsList.tsx`
- `src/WhatsAppPlatform.Api/ClientApp/src/organizations/organizationsApi.ts`
- `src/WhatsAppPlatform.Api/ClientApp/src/whatsappAccounts/WhatsAppAccountsSection.tsx`
- `src/WhatsAppPlatform.Api/ClientApp/src/whatsappAccounts/whatsappApi.ts`
- `src/WhatsAppPlatform.Api/Identity/AntiforgeryMiddleware.cs`
- `src/WhatsAppPlatform.Api/Identity/AuthenticationEndpoints.cs`
- `src/WhatsAppPlatform.Api/Identity/CookieIdentitySession.cs`
- `src/WhatsAppPlatform.Api/Identity/HttpCurrentUser.cs`
- `src/WhatsAppPlatform.Api/Identity/IdentityRegistration.cs`
- `src/WhatsAppPlatform.Api/Identity/OrganizationAccessFilter.cs`
- `src/WhatsAppPlatform.Api/Identity/UnknownUserPasswordCheck.cs`
- `src/WhatsAppPlatform.Api/Organizations/CreateOrganizationEndpoint.cs`
- `src/WhatsAppPlatform.Api/Organizations/GetOrganizationDetailsEndpoint.cs`
- `src/WhatsAppPlatform.Api/Program.cs`
- `src/WhatsAppPlatform.Api/WhatsAppAccounts/CompleteOnboardingEndpoint.cs`
- `src/WhatsAppPlatform.Api/WhatsAppAccounts/GetOnboardingSessionEndpoint.cs`
- `src/WhatsAppPlatform.Api/WhatsAppAccounts/GetWhatsAppAccountEndpoint.cs`
- `src/WhatsAppPlatform.Api/WhatsAppAccounts/ListWhatsAppAccountsEndpoint.cs`
- `src/WhatsAppPlatform.Api/WhatsAppAccounts/StartOnboardingSessionEndpoint.cs`
- `src/WhatsAppPlatform.Api/packages.lock.json`
- `src/WhatsAppPlatform.Application/Identity/Contracts/ICurrentUser.cs`
- `src/WhatsAppPlatform.Application/Identity/Contracts/IMembershipStore.cs`
- `src/WhatsAppPlatform.Application/Identity/Contracts/IOrganizationAccessService.cs`
- `src/WhatsAppPlatform.Application/Identity/Contracts/ITenantResourceOwnership.cs`
- `src/WhatsAppPlatform.Application/Identity/OrganizationAccessDecision.cs`
- `src/WhatsAppPlatform.Application/Identity/OrganizationAccessService.cs`
- `src/WhatsAppPlatform.Application/Identity/README.md`
- `src/WhatsAppPlatform.Application/Organizations/Contracts/IOrganizationStore.cs`
- `src/WhatsAppPlatform.Application/Organizations/ListOrganizations/ListOrganizationsHandler.cs`
- `src/WhatsAppPlatform.Domain/Identity/Contracts/OrganizationMembershipId.cs`
- `src/WhatsAppPlatform.Domain/Identity/Contracts/OrganizationRole.cs`
- `src/WhatsAppPlatform.Domain/Identity/Contracts/UserId.cs`
- `src/WhatsAppPlatform.Domain/Identity/OrganizationMembership.cs`
- `src/WhatsAppPlatform.Domain/Identity/README.md`
- `src/WhatsAppPlatform.Infrastructure/DependencyInjection.cs`
- `src/WhatsAppPlatform.Infrastructure/Identity/BootstrapIdentityProvisioner.cs`
- `src/WhatsAppPlatform.Infrastructure/Identity/EfMembershipStore.cs`
- `src/WhatsAppPlatform.Infrastructure/Identity/EfTenantResourceOwnership.cs`
- `src/WhatsAppPlatform.Infrastructure/Identity/IdentityModelConfiguration.cs`
- `src/WhatsAppPlatform.Infrastructure/Identity/IdentityPersistenceRegistration.cs`
- `src/WhatsAppPlatform.Infrastructure/Identity/MembershipConfiguration.cs`
- `src/WhatsAppPlatform.Infrastructure/Identity/MembershipRecord.cs`
- `src/WhatsAppPlatform.Infrastructure/Identity/Migrations/20261006162215_AddIdentityAndOrganizationMemberships.Designer.cs`
- `src/WhatsAppPlatform.Infrastructure/Identity/Migrations/20261006162215_AddIdentityAndOrganizationMemberships.cs`
- `src/WhatsAppPlatform.Infrastructure/Identity/PlatformUser.cs`
- `src/WhatsAppPlatform.Infrastructure/Organizations/EfOrganizationStore.cs`
- `src/WhatsAppPlatform.Infrastructure/Organizations/Migrations/PlatformDbContextModelSnapshot.cs`
- `src/WhatsAppPlatform.Infrastructure/Persistence/PlatformDbContext.cs`
- `src/WhatsAppPlatform.Infrastructure/WhatsAppPlatform.Infrastructure.csproj`
- `src/WhatsAppPlatform.Infrastructure/packages.lock.json`
- `tests/WhatsAppPlatform.Tests/AggregateIdTests.cs`
- `tests/WhatsAppPlatform.Tests/OrganizationAccessServiceTests.cs`
- `tests/WhatsAppPlatform.Tests/OrganizationMembershipTests.cs`
- `tests/WhatsAppPlatform.Tests/packages.lock.json`
