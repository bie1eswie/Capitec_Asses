using Bams.Application;
using Bams.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bams.Infrastructure;

public sealed class ServiceRepository(BamsDbContext db) : IServiceRepository
{
    public async Task<IReadOnlyList<Service>> ListActiveAsync(CancellationToken ct) =>
        await db.Services.AsNoTracking().Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync(ct);

    public Task<Service?> FindActiveByIdAsync(Guid id, CancellationToken ct) =>
        db.Services.AsNoTracking().FirstOrDefaultAsync(service => service.Id == id && service.IsActive, ct);
}
