namespace Bams.Infrastructure.Persistence.Entities;

public sealed class BranchMembership
{
    public Guid Id { get; set; }

    public required string UserId { get; set; }

    public Guid BranchId { get; set; }

    public BranchMembershipRole Role { get; set; }
}
