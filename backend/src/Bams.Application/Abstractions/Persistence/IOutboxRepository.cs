using Bams.Domain;

namespace Bams.Application;

public interface IOutboxRepository
{
    void Add(OutboxEvent item);

    Task<IReadOnlyList<OutboxEvent>> ClaimPendingAsync(int limit, DateTimeOffset now, TimeSpan leaseDuration, CancellationToken cancellationToken, int maximumAttempts = 10);

    Task MarkPublishedAsync(Guid eventId, DateTimeOffset publishedAt, CancellationToken cancellationToken);

    Task MarkFailedAsync(Guid eventId, DateTimeOffset nextAttemptAt, string error, bool deadLetter, CancellationToken cancellationToken);

    Task<bool> ReplayDeadLetterAsync(Guid eventId, CancellationToken cancellationToken);
}
