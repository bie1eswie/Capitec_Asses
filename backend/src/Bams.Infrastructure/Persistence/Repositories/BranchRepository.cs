using Bams.Application;
using Bams.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bams.Infrastructure;

public sealed class BranchRepository(BamsDbContext db) : IBranchRepository
{
    public async Task<IReadOnlyList<Branch>> ListActiveAsync(string? search, CancellationToken ct)
    {
        var query = db.Branches.AsNoTracking().Where(b => b.Status == BranchStatus.Active);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(b => EF.Functions.ILike(b.Name, $"%{term}%") || EF.Functions.ILike(b.City, $"%{term}%"));
        }
        return await query.OrderBy(b => b.Name).Take(100).ToListAsync(ct);
    }

    public Task<Branch?> FindActiveByIdAsync(Guid id, CancellationToken ct) => db.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id && b.Status == BranchStatus.Active, ct);

    public Task<BranchService?> FindBranchServiceAsync(Guid branchId, Guid serviceId, CancellationToken ct) =>
        db.BranchServices.AsNoTracking().FirstOrDefaultAsync(
            branchService => branchService.BranchId == branchId && branchService.ServiceId == serviceId,
            ct);

    public async Task<IReadOnlyList<Service>> ListActiveServicesAsync(Guid branchId, CancellationToken ct) => await db.BranchServices.AsNoTracking().Where(bs => bs.BranchId == branchId && bs.Branch.Status == BranchStatus.Active && bs.IsEnabled && bs.Service.IsActive).OrderBy(bs => bs.Service.Name).Select(bs => bs.Service).ToListAsync(ct);
}
