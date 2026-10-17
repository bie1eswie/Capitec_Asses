using Bams.Application;
using Bams.Application.Abstractions.Communications;
using Microsoft.Extensions.Options;

namespace Bams.Infrastructure;

public sealed class PostgreSqlEventSubscriber(
    IOutboxRepository outboxRepository,
    IOptions<MessagingOptions> options) : IEventSubscriber
{
    private readonly Dictionary<Guid, int> attempts = [];

    public async Task<EventEnvelope?> ReceiveAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var events = await outboxRepository.ClaimPendingAsync(
            1,
            DateTimeOffset.UtcNow,
            TimeSpan.FromSeconds(settings.LeaseDurationSeconds),
            cancellationToken,
            settings.MaximumAttempts);
        var item = events.SingleOrDefault();
        if (item is null)
        {
            return null;
        }

        attempts[item.Id] = item.Attempts;
        return new EventEnvelope(
            item.Id,
            item.EventType,
            item.Version,
            item.CorrelationId ?? item.Id.ToString("N"),
            item.OccurredAt,
            item.PayloadJson);
    }

    public async Task AcknowledgeAsync(EventEnvelope envelope, CancellationToken cancellationToken)
    {
        attempts.Remove(envelope.EventId);
        await outboxRepository.MarkPublishedAsync(
            envelope.EventId,
            DateTimeOffset.UtcNow,
            cancellationToken);
    }

    public async Task RejectAsync(EventEnvelope envelope, bool retry, string failureDetails, CancellationToken cancellationToken)
    {
        attempts.TryGetValue(envelope.EventId, out var attemptCount);
        attempts.Remove(envelope.EventId);
        var deadLetter = !retry || attemptCount + 1 >= options.Value.MaximumAttempts;
        var delaySeconds = Math.Min(Math.Pow(2, attemptCount), 300);
        await outboxRepository.MarkFailedAsync(
            envelope.EventId,
            DateTimeOffset.UtcNow.AddSeconds(delaySeconds),
            failureDetails,
            deadLetter,
            cancellationToken);
    }
}
