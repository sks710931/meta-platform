# Meta Embedded Signup contract verification blocker

Real Embedded Signup v4 is not implemented. The permitted Development form cleanup removes
three stale numeric-only HTML patterns; required fields, length limits, authentication,
organization administration, antiforgery, endpoint contracts, and manual flow remain unchanged.

## Required contract decision

The existing WhatsApp Accounts model requires a separate ExternalMessagingAccountId for
every MessagingAccount, describing a Meta messaging/payment identity distinct from the WABA.
Successful registration creates this record atomically with the account, phone numbers, and
completed session. Real onboarding cannot supply an invented ID, copy the WABA ID into it,
or reinterpret the entity without an explicit domain decision.

The task requires current official Meta documentation to establish that mapping and explicitly
requires stopping before changing domain meaning if the documented model differs. No mapping
has been verified. This is an unresolved contract requirement, not a claim that Meta definitely
has no such resource. We must establish whether an independently addressable messaging/payment
entity exists in the customer flow, what its documented identifier is, and which authoritative
server-side response exposes it. If the current provider model differs, report that discrepancy
and agree on the model change before implementing registration.

## Documentation access

The official documentation URL attempted was:
https://developers.facebook.com/docs/whatsapp/embedded-signup/

The managed environment's proxy rejected the HTTPS CONNECT request with HTTP 403; no Meta
documentation body was retrieved. The network policy did not allow developers.facebook.com.
There is no available web-search/browser research connector in this session. Neither SDK v4
parameters/events, code exchange, token type/expiry, resource discovery, nor the required
MessagingAccount mapping has been verified against current official documentation.

To resume, make current official documentation accessible or provide its relevant authoritative
text. Verify the v4 SDK configuration/result contract, backend exchange and customer credential
type, authorization/scoping checks, WABA and phone discovery, and MessagingAccount mapping.
Do not infer contracts from older implementation memories or invent Graph endpoints/fields.

## Deferred implementation sequence (design only)

React → API: start session
React → Meta SDK: Embedded Signup v4
Meta → React: transient authorization result
React → API: authorization result
API → Meta: exchange code once
Meta → API: customer credential
API → Credential Store: protect/persist immediately
API → Meta: discover and verify authoritative resources
API → PostgreSQL: account graph plus completed session
API → React: connected account

The authorization result must remain transient in browser memory. The secret must never
enter Domain, API responses, React configuration, or logs. A dedicated Infrastructure record
would persist only Data Protection ciphertext, using a dedicated purpose and the protected
persistent key ring; expiry metadata must come from documented provider semantics.
Customer Meta credentials and future platform Solution Partner/system-user credentials are
separate security contexts. Do not add business_management to this customer flow by assumption.
The customer permissions specified by the task are whatsapp_business_messaging and
whatsapp_business_management; their exact v4 configuration remains to be verified.

The planned failure window is explicit: code exchange cannot share a PostgreSQL transaction.
After exchange, protect/persist the credential before remaining discovery. If persistence fails,
require signup restart; do not retry a consumed code. Once completion is known, replay must use
persisted server state without exchanging again. The future production completion route must
reuse stored session ownership, OrganizationAdmin/PlatformAdmin authorization, antiforgery,
unknown-field rejection, existing uniqueness guarantees, and atomic account-graph registration.

## Scope and security review

No Meta configuration, SDK wrapper, HTTP gateway, production completion endpoint, credential
record, or migration was added while these contracts remain unverified. No Meta provider API
was called; only official documentation access was attempted. No App Secret, authorization
code, or access token was introduced into source, React-visible data, Domain, API responses,
logs, committed configuration, or documentation. Existing authorization/persistence foundations
are unchanged. No new tests or integration infrastructure are warranted for this HTML cleanup.

Future tests after contracts are verified should cover pure authoritative result mapping,
sanitized provider-error classification, persisted-state replay decisions, secret exclusion,
and real sandbox exchange/discovery, scope checks, tenant authorization, and failure-window
recovery. Provider/SDK behavior belongs in future real integration scenarios, not large mocks.

## Supported resolution prepared

The user requested finding an alternative rather than supplying documentation. Meta's official
fbsamples/whatsapp-api-examples repository was fetched through the permitted GitHub route,
including HEAD de70ee908a67026e642aaee3703d20464e2a9466. Its current README/documentation
search did not establish Embedded Signup v4 or an independent MessagingAccount provider ID;
examples alone cannot justify inventing the missing API contract.

A targeted environment network draft was saved successfully, preserving existing custom
rules and the package-manager preset. It adds developers.facebook.com, developers.meta.com,
www.facebook.com, www.postman.com, graph.facebook.com, and connect.facebook.net. These cover
official documentation (including potential canonical redirects and Meta's Postman workspace),
Graph calls, and the official SDK. No unrestricted network rule or credential requirement was
added. The configuration tool returned requires_publish=true: the user must review/save the
draft in environment settings and publish it before the agent can rely on updated runtime
access. Draft saving is not proof of live access. No network restriction was bypassed.

## Completed checks

The .NET build passed with zero warnings/errors and all 29 existing unit cases passed, none
skipped. React standalone TypeScript and production build passed. EF reports no pending model
changes; no migration is needed for this HTML-only cleanup. No tests were added or changed.
A targeted source search found no stale numeric HTML patterns, AppSecret, authorization-code,
or access-token fields in React, Domain, appsettings, or committed environment examples.
The real implementation and its full integration security verification remain outstanding.
