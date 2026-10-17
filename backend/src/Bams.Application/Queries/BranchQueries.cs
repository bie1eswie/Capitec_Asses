namespace Bams.Application;

public sealed class BranchQueries(IBranchRepository repository) : IBranchQueries
{
    public async Task<IReadOnlyList<BranchDto>> ListAsync(string? search, CancellationToken ct) => (await repository.ListActiveAsync(search, ct)).Select(Map).ToList();

    public async Task<BranchDto?> FindAsync(Guid id, CancellationToken ct)
    {
        var branch = await repository.FindActiveByIdAsync(id, ct);
        return branch is null ? null : Map(branch);
    }

    public async Task<IReadOnlyList<ServiceDto>> ServicesAsync(Guid branchId, CancellationToken ct) => (await repository.ListActiveServicesAsync(branchId, ct)).Select(s => new ServiceDto(s.Id, s.Code, s.Name, s.DurationMinutes)).ToList();

    private static BranchDto Map(Bams.Domain.Branch b) => new(b.Id, b.Code, b.Name, b.City, b.Address, b.TimeZone);
}
