using Bams.Domain;

namespace Bams.Application;

public interface IServiceRepository
{
    Task<IReadOnlyList<Service>> ListActiveAsync(CancellationToken cancellationToken);
    Task<Service?> FindActiveByIdAsync(Guid id, CancellationToken cancellationToken);
}
