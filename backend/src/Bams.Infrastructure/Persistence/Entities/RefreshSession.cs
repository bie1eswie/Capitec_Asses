namespace Bams.Infrastructure.Persistence.Entities;

public sealed class RefreshSession
{
    public Guid Id { get; set; }

    public required string UserId { get; set; }

    public Guid FamilyId { get; set; }

    public required string TokenHash { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public Guid? ReplacedBySessionId { get; set; }
}
