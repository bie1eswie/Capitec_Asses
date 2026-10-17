using Bams.Application;
using Bams.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Bams.Infrastructure;

public sealed class UnitOfWork(BamsDbContext db, ILogger<UnitOfWork> logger) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);

    private const int MaximumConcurrencyAttempts = 3;

    public async Task<T> ExecuteLockedAsync<T>(
        Guid branchId,
        Guid serviceId,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken,
        string? operationLockKey = null)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await ExecuteAttemptAsync(
                    branchId,
                    serviceId,
                    operation,
                    cancellationToken,
                    operationLockKey);
            }
            catch (PostgresException exception) when (IsRetryableConcurrencyFailure(exception) && attempt < MaximumConcurrencyAttempts)
            {
                await RetryTransactionAsync(exception, branchId, serviceId, attempt, cancellationToken);
            }
            catch (DbUpdateException exception) when (ContainsRetryablePostgresFailure(exception) && attempt < MaximumConcurrencyAttempts)
            {
                await RetryTransactionAsync(exception, branchId, serviceId, attempt, cancellationToken);
            }
            catch (DbUpdateConcurrencyException exception) when (attempt < MaximumConcurrencyAttempts)
            {
                await RetryTransactionAsync(exception, branchId, serviceId, attempt, cancellationToken);
            }
        }
    }

    private static bool ContainsRetryablePostgresFailure(DbUpdateException exception)
    {
        for (var inner = exception.InnerException; inner is not null; inner = inner.InnerException)
        {
            if (inner is PostgresException postgresException && IsRetryableConcurrencyFailure(postgresException))
            {
                return true;
            }
        }

        return false;
    }

    private async Task RetryTransactionAsync(
        Exception exception,
        Guid branchId,
        Guid serviceId,
        int attempt,
        CancellationToken cancellationToken)
    {
        logger.LogWarning(
            exception,
            "Retrying transaction for branch {BranchId} service {ServiceId}, attempt {Attempt}",
            branchId,
            serviceId,
            attempt + 1);
        db.ChangeTracker.Clear();
        await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt), cancellationToken);
    }

    private async Task<T> ExecuteAttemptAsync<T>(
        Guid branchId,
        Guid serviceId,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken,
        string? operationLockKey)
    {
        logger.LogDebug("Attempting to acquire lock for branch {BranchId} service {ServiceId}", branchId, serviceId);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(operationLockKey))
        {
            await AcquireOperationLockAsync(operationLockKey, cancellationToken);
        }

        var locked = await db.BranchServices
            .FromSqlInterpolated($"SELECT * FROM branch_services WHERE branch_id = {branchId} AND service_id = {serviceId} FOR UPDATE")
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);
        if (locked is null)
        {
            throw new BookingException("Branch service is unavailable.", 404);
        }

        db.ChangeTracker.Clear();
        var result = await operation(cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        logger.LogDebug("Released lock and committed transaction for branch {BranchId} service {ServiceId}", branchId, serviceId);
        return result;
    }

    private static bool IsRetryableConcurrencyFailure(PostgresException exception)
    {
        return exception.SqlState is PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected;
    }

    private async Task AcquireOperationLockAsync(string operationLockKey, CancellationToken cancellationToken)
    {
        var connection = (NpgsqlConnection)db.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.Transaction =
            (NpgsqlTransaction)db.Database.CurrentTransaction!.GetDbTransaction();
        command.CommandText = "SELECT pg_advisory_xact_lock(hashtextextended(@lock_key, 0))";
        command.Parameters.AddWithValue("lock_key", operationLockKey);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
