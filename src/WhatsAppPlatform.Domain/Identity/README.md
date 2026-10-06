# Identity

Owns UserId, OrganizationMembershipId, OrganizationRole, and the independent
OrganizationMembership domain entity. Membership contains typed user/organization references,
a defined organization role, and a UTC creation time. PlatformAdmin is a platform-level
Identity persistence role, not a membership or Organization domain concept.
No ASP.NET, EF Core, ClaimsPrincipal, password, or cookie types enter this context's Domain.
