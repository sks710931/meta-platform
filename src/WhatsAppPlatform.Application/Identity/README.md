# Identity

Owns the current-user, membership, organization-access, and tenant-resource ownership contracts.
OrganizationAccessService coordinates current user and membership reads; its small decision
functions distinguish read access from administration without a generic permission engine.
HTTP claims/cookies remain in API; database and Identity framework concerns remain in Infrastructure.
