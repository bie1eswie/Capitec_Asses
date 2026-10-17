using Bams.Application.Abstractions.Communications;
using Bams.Application;
using Bams.Domain;
using System.Text.Json;

namespace Bams.Infrastructure;

public sealed class PostgreSqlEventPublisher(IOutboxRepository outboxRepository) : IEventPublisher
{
    public Task PublishAsync(EventEnvelope envelope, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        outboxRepository.Add(new OutboxEvent
        {
            Id = envelope.EventId,
            EventType = envelope.EventName,
            Version = envelope.Version,
            CorrelationId = envelope.CorrelationId,
            PayloadJson = NormalizePayload(envelope.Payload),
            OccurredAt = envelope.OccurredAt
        });
        return Task.CompletedTask;
    }

    private static string NormalizePayload(string payload)
    {
        using var document = JsonDocument.Parse(payload);
        return document.RootElement.GetRawText();
    }
}
