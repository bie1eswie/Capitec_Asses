namespace Bams.Application.Abstractions.Security;

public interface IBranchMembershipRepository
{
    Task<bool> HasMembershipAsync(
        string userId,
        Guid branchId,
        string role,
        CancellationToken cancellationToken);
}
