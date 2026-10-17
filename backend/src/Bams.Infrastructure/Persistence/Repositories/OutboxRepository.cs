using Bams.Application;
using Bams.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bams.Infrastructure;

public sealed class OutboxRepository(BamsDbContext db) : IOutboxRepository
{
    public void Add(OutboxEvent item) => db.OutboxEvents.Add(item);

    public async Task<IReadOnlyList<OutboxEvent>> ClaimPendingAsync(int limit, DateTimeOffset now, TimeSpan leaseDuration, CancellationToken cancellationToken, int maximumAttempts = 10)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var events = await db.OutboxEvents
            .FromSqlInterpolated($"""
                SELECT * FROM outbox_events
                WHERE published_at IS NULL
                  AND dead_lettered = FALSE
                  AND attempts < {maximumAttempts}
                  AND (next_attempt_at IS NULL OR next_attempt_at <= {now})
                  AND (locked_until IS NULL OR locked_until <= {now})
                ORDER BY occurred_at
                FOR UPDATE SKIP LOCKED
                LIMIT {limit}
                """)
            .ToListAsync(cancellationToken);

        foreach (var item in events)
        {
            item.LockedUntil = now.Add(leaseDuration);
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return events;
    }

    public async Task MarkPublishedAsync(Guid eventId, DateTimeOffset publishedAt, CancellationToken cancellationToken)
    {
        var item = await db.OutboxEvents.SingleOrDefaultAsync(x => x.Id == eventId, cancellationToken);
        if (item is null)
        {
            return;
        }

        item.PublishedAt = publishedAt;
        item.LockedUntil = null;
        item.LastError = null;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(Guid eventId, DateTimeOffset nextAttemptAt, string error, bool deadLetter, CancellationToken cancellationToken)
    {
        var item = await db.OutboxEvents.SingleOrDefaultAsync(x => x.Id == eventId, cancellationToken);
        if (item is null)
        {
            return;
        }

        item.Attempts++;
        item.LastError = error.Length > 2000 ? error[..2000] : error;
        item.DeadLettered = deadLetter;
        item.NextAttemptAt = deadLetter ? null : nextAttemptAt;
        item.LockedUntil = null;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ReplayDeadLetterAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var item = await db.OutboxEvents.SingleOrDefaultAsync(x => x.Id == eventId && x.DeadLettered, cancellationToken);
        if (item is null)
        {
            return false;
        }

        item.Attempts = 0;
        item.DeadLettered = false;
        item.NextAttemptAt = null;
        item.LockedUntil = null;
        item.LastError = null;
        item.PublishedAt = null;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
