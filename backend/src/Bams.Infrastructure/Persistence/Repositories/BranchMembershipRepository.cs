using Bams.Application.Abstractions.Security;
using Bams.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bams.Infrastructure;

public sealed class BranchMembershipRepository(BamsDbContext db) : IBranchMembershipRepository
{
    public Task<bool> HasMembershipAsync(
        string userId,
        Guid branchId,
        string role,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<BranchMembershipRole>(role, ignoreCase: false, out var membershipRole))
        {
            return Task.FromResult(false);
        }

        return db.BranchMemberships.AnyAsync(
            membership => membership.UserId == userId &&
                          membership.BranchId == branchId &&
                          membership.Role == membershipRole,
            cancellationToken);
    }
}
