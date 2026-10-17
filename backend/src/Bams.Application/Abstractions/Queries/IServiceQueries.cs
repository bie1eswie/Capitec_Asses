namespace Bams.Application;

public interface IServiceQueries
{
    Task<IReadOnlyList<ServiceDto>> ListAsync(CancellationToken cancellationToken);
}
