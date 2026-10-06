# Domain model

This describes intended ownership and boundaries. Only typed identifier contracts are
implemented; aggregates, workflows, policies, and persistence tables are future work.

| Term | Owner | Intended boundary and references |
| --- | --- | --- |
| Organization | Organizations | Customer/tenant root identified by `OrganizationId`; no collection of WhatsApp accounts |
| WhatsApp Account | WhatsApp Accounts | Small root identified by `WhatsAppAccountId`, referencing `OrganizationId`; separate external Meta WABA identifier |
| Phone Number | WhatsApp Accounts | Independently addressable root identified by `PhoneNumberId`, referencing its `WhatsAppAccountId` and tenant; separate Meta phone-number identifier |
| Messaging Account | Messaging | Messaging/payment entity associated with a connected account; `MessagingAccountId`, `WhatsAppAccountId`, and `OrganizationId` references |
| Credit Line Assignment | Billing | Small root identified by `CreditLineAssignmentId`, referencing customer `MessagingAccountId` and `OrganizationId`; records platform/Solution Partner credit-line association |
| Platform user/membership | Identity | Future identity and authorization model; organization membership expressed by ID rather than embedding Organization |
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
- No Meta or WhatsApp operations, Organization CRUD, or aggregate behavior are introduced in this iteration.
