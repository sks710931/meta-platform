# Meta Embedded Signup v4 contract review

Official Meta documentation was retrieved on 2026-10-06 through the environment's normal
HTTPS proxy. The earlier documentation-access blocker is resolved. The provider/domain discrepancy below was reported before implementation. The user explicitly
approved deferring the separate WAAC identity and storing the verified `waba_id` on
MessagingAccount; the implementation now follows that targeted adjustment.

## Verified account-model discrepancy

Before the approved adjustment, the repository required both `WhatsAppAccount.ExternalWhatsAppAccountId` and
`MessagingAccount.ExternalMessagingAccountId` when registering an account graph. Both were
non-null, globally unique external identifiers; internal typed IDs remain the primary keys.

Meta's [account-model overview](https://developers.facebook.com/documentation/business-messaging/whatsapp/account-model-evolution/)
describes the split of the legacy WABA into a WhatsApp Account (WAAC, phone identity) and a
Messaging Account (templates, billing, and webhook subscriptions). It states:

> WhatsApp Business Accounts have their own IDs, but these are not available until
> Phase 2 — New Graph API version.

Its [onboarding changes](https://developers.facebook.com/documentation/business-messaging/whatsapp/account-model-evolution/onboarding/)
state:

> With the new account model, onboarding flows automatically create a separate Messaging
> Account per partner and return the ID in the existing WABA ID field.

The published Phase 1 schedule is H2 2026: `waba_id` represents the Messaging Account ID,
while `waac_id` is not included. Phase 2, planned for H1 2027, introduces the distinct WAAC
identifier and new APIs. The overview labels this model **beta and subject to change**;
these dates do not prove rollout for a particular app or customer. Legacy WABA handling
and actual app availability must be verified before deployment.

Thus Messaging Account is a documented concept, but current signup does not supply the
separate WAAC identity required by the existing registration contract. Do not invent that
identity, substitute a phone ID, or copy the Messaging Account ID into it while claiming
it identifies a separate WAAC. Meta also describes one phone per WAAC and potentially
multiple Messaging Accounts per phone; the current local connection root groups phones
and one MessagingAccount. A local aggregate need not mirror Meta's hierarchy, but that
local meaning must be explicit.

## Approved minimal decision

Keep the tenant-owned internal WhatsAppAccount aggregate as the local connected-account
root. Defer its external WAAC identity (make it optional until Meta exposes/verifies it),
and store the verified current `waba_id` in `MessagingAccount.ExternalMessagingAccountId`.
Keep internal IDs, OrganizationId ownership, phone identifiers, session transitions,
atomic registration, uniqueness, and replay/concurrency guarantees.

This adjustment is implemented with a nullable WAAC column and response field. Existing
Development/manual identifiers are retained as unverified simulation values, never used
as authoritative Meta discovery inputs. Existing WABA Graph paths continue to address the
Messaging Account/predecessor resource; no WAAC endpoint or unavailable ID is invented.
The local connection root groups the discovered phones without claiming to mirror the
new one-phone-per-WAAC provider hierarchy. Multi-Messaging-Account phone sharing remains
outside this slice because existing external-phone uniqueness is preserved.

## Other verified v4 contracts

- [Embedded Signup v4](https://developers.facebook.com/documentation/business-messaging/whatsapp/embedded-signup/version-4):
  select v4 through a Facebook Login for Business configuration with the Embedded Signup
  variation and WhatsApp assets. Do not invent a v4 SDK extras parameter.
- [Implementation](https://developers.facebook.com/documentation/business-messaging/whatsapp/embedded-signup/implementation):
  load the official SDK at `https://connect.facebook.net/en_US/sdk.js`; initialize with
  AppId and Graph version. Login uses `config_id`, `response_type: "code"`,
  `override_default_response_type: true`, and `extras: { setup: {} }`.
  The callback returns `authResponse.code`; documentation states a 30-second code TTL.
  `WA_EMBEDDED_SIGNUP` events supply browser-side asset hints and completion/cancel/error
  state. Closing the final confirmation screen can be successful, so popup closure alone
  must not be classified as cancellation. Browser asset hints are not ownership evidence.
- [Customer onboarding](https://developers.facebook.com/documentation/business-messaging/whatsapp/embedded-signup/onboarding-customers-as-a-tech-provider):
  server exchange uses `GET /{version}/oauth/access_token` with `client_id`, `client_secret`,
  and `code`. It yields a **business integration system user access token**, also called
  a business token. Never log the request URL or response; do not retry a consumed code.
- [WABA node reference](https://developers.facebook.com/documentation/business-messaging/whatsapp/reference/whatsapp-business-account/whatsapp-business-account-api):
  documents `GET /{version}/{WABA-ID}` and account fields, but does not provide a current
  distinct WAAC ID to satisfy this model. Phone discovery and token scoping use the official references below; no separate
  WAAC lookup is performed.
- [Embedded Signup overview](https://developers.facebook.com/docs/whatsapp/embedded-signup/):
  customer Cloud API access requires `whatsapp_business_management` and
  `whatsapp_business_messaging`. A platform Solution Partner/system-user credential for
  credit-line operations is a separate credential/security context; do not request its
  `business_management` permission as part of this customer flow.

The application now implements exchange, debug-token verification, account-node lookup,
and phone discovery using these documented paths. No live credential exchange, SDK signup,
or real customer onboarding was executed in this environment.
No provider sample secrets or authorization codes were copied into the repository.

## Implemented sequence and failure window

```text
React → API: start session
React → Meta SDK: Embedded Signup v4
Meta → React: transient authorization result
React → API: authorization result
API → Meta: exchange code once
Meta → API: customer credential
API → Credential Store: protect/persist immediately
API → Meta: discover/verify authoritative resources
API → PostgreSQL: account graph + completed session
API → React: connected account
```

Keep the code only in transient memory until immediate submission. A dedicated Infrastructure
credential record must store only Data Protection ciphertext with a dedicated purpose and
persistent protected key ring. No secrets belong in Domain, React configuration, responses,
or logs. Store expiry only when supported by the actual documented exchange response.

Code exchange and PostgreSQL cannot share a transaction. Protect/persist the credential before
remaining discovery. If exchange consumes the code but storage fails, require signup restart.
Completed-session replay must use persisted server state without another code exchange.
The production route resolves stored session ownership, enforces OrganizationAdmin or
PlatformAdmin access and antiforgery, rejects unknown fields, and reuses atomic registration.

Full messaging readiness remains outside scope: webhook subscriptions, phone registration,
payment/credit-line operations, sending, and billing are not implemented by this review.

## Additional official references

- [Debug token](https://developers.facebook.com/docs/graph-api/reference/debug_token/):
  `GET /{version}/debug_token`, app access-token authorization, `app_id`, `is_valid`,
  `scopes`, `granular_scopes.scope/target_ids`, `expires_at`, and `data_access_expires_at`.
  Granular targets may be absent for unrestricted access; this implementation fails closed.
- [Access token guide](https://developers.facebook.com/docs/facebook-login/guides/access-tokens/):
  documents the server-only AppId/AppSecret app-token form used to inspect the customer token.
- [Phone numbers](https://developers.facebook.com/documentation/business-messaging/whatsapp/business-phone-numbers/phone-numbers):
  `GET /{version}/{WABA-ID}/phone_numbers`, `id`, `display_phone_number`, and `verified_name`.
- [Graph pagination](https://developers.facebook.com/docs/graph-api/results/):
  cursor-based `paging.cursors.after` and `paging.next`; requests stay on the fixed Graph host.

See [implementation and verification](meta-embedded-signup.md) for configuration, safe DTOs,
credential checkpoints, authorization, deliberate limitations, and the exact test list.
