using WhatsAppPlatform.Domain.Organizations.Contracts;

namespace WhatsAppPlatform.Domain.Organizations;

public sealed class Organization
{
    public const int MaximumNameLength = 200;

    public OrganizationId Id { get; }
    public string Name { get; }
    public OrganizationStatus Status { get; }
    public DateTimeOffset CreatedAt { get; }

    private Organization(OrganizationId id, string name, OrganizationStatus status, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        Status = status;
        CreatedAt = createdAt;
    }

    public static OrganizationCreationResult Create(OrganizationId id, string? name, DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(id);
        if (string.IsNullOrWhiteSpace(name) || name.Contains('\0'))
        {
            return new OrganizationCreationResult.Invalid("Name must contain text and cannot contain null characters.");
        }

        var normalizedName = string.Join(' ', name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (normalizedName.Length > MaximumNameLength)
        {
            return new OrganizationCreationResult.Invalid($"Name must be at most {MaximumNameLength} characters.");
        }

        if (createdAt.Offset != TimeSpan.Zero)
        {
            return new OrganizationCreationResult.Invalid("Creation timestamp must be UTC.");
        }

        return new OrganizationCreationResult.Created(new Organization(id, normalizedName, OrganizationStatus.Active, createdAt));
    }
}
