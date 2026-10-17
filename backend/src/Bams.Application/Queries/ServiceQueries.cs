namespace Bams.Application;

public sealed class ServiceQueries(IServiceRepository repository) : IServiceQueries
{
    public async Task<IReadOnlyList<ServiceDto>> ListAsync(CancellationToken ct) => (await repository.ListActiveAsync(ct)).Select(s => new ServiceDto(s.Id, s.Code, s.Name, s.DurationMinutes)).ToList();
}
