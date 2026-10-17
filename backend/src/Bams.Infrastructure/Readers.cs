using Bams.Application;
using Bams.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bams.Infrastructure;

public sealed class BranchReader(BamsDbContext db) : IBranchReader
{
    public async Task<IReadOnlyList<BranchDto>> ListAsync(string? search, CancellationToken ct)
    {
        var q = db.Branches.AsNoTracking().Where(b => b.Status == BranchStatus.Active);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(b => EF.Functions.ILike(b.Name, $"%{term}%") || EF.Functions.ILike(b.City, $"%{term}%"));
        }
        return await q.OrderBy(b => b.Name).Take(100)
            .Select(b => new BranchDto(b.Id, b.Code, b.Name, b.City, b.Address, b.TimeZone)).ToListAsync(ct);
    }
    public Task<BranchDto?> FindAsync(Guid id, CancellationToken ct) => db.Branches.AsNoTracking()
        .Where(b => b.Id == id && b.Status == BranchStatus.Active)
        .Select(b => new BranchDto(b.Id, b.Code, b.Name, b.City, b.Address, b.TimeZone)).FirstOrDefaultAsync(ct);
    public async Task<IReadOnlyList<ServiceDto>> ServicesAsync(Guid branchId, CancellationToken ct) =>
        await db.BranchServices.AsNoTracking()
            .Where(bs => bs.BranchId == branchId && bs.Branch.Status == BranchStatus.Active && bs.IsEnabled && bs.Service.IsActive)
            .OrderBy(bs => bs.Service.Name)
            .Select(bs => new ServiceDto(bs.ServiceId, bs.Service.Code, bs.Service.Name, bs.Service.DurationMinutes))
            .ToListAsync(ct);
}
public sealed class ServiceReader(BamsDbContext db) : IServiceReader
{
    public async Task<IReadOnlyList<ServiceDto>> ListAsync(CancellationToken ct) =>
        await db.Services.AsNoTracking().Where(s => s.IsActive).OrderBy(s => s.Name)
            .Select(s => new ServiceDto(s.Id, s.Code, s.Name, s.DurationMinutes)).ToListAsync(ct);
}
