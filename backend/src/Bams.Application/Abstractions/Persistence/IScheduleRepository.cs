namespace Bams.Application;

public interface IScheduleRepository
{
    Task<bool> IsOpenAsync(
        Guid branchId,
        DateTimeOffset start,
        DateTimeOffset end,
        string timeZone,
        CancellationToken cancellationToken);
}
