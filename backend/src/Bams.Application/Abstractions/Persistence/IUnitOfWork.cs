namespace Bams.Application;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);

    Task<T> ExecuteLockedAsync<T>(
        Guid branchId,
        Guid serviceId,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken,
        string? operationLockKey = null);
}
