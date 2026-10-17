namespace Bams.Application;

public interface IBranchReader
{
    Task<IReadOnlyList<BranchDto>> ListAsync(string? search, CancellationToken cancellationToken);
    Task<BranchDto?> FindAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ServiceDto>> ServicesAsync(Guid branchId, CancellationToken cancellationToken);
}
public interface IServiceReader
{
    Task<IReadOnlyList<ServiceDto>> ListAsync(CancellationToken cancellationToken);
}
