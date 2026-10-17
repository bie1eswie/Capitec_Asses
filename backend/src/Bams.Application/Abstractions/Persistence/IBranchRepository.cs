using Bams.Domain;

namespace Bams.Application;

public interface IBranchRepository
{
    Task<IReadOnlyList<Branch>> ListActiveAsync(string? search, CancellationToken cancellationToken);
    Task<Branch?> FindActiveByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<BranchService?> FindBranchServiceAsync(Guid branchId, Guid serviceId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Service>> ListActiveServicesAsync(Guid branchId, CancellationToken cancellationToken);
}
