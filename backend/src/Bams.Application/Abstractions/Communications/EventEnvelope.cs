namespace Bams.Application.Abstractions.Communications;

public sealed record EventEnvelope(
    Guid EventId,
    string EventName,
    int Version,
    string CorrelationId,
    DateTimeOffset OccurredAt,
    string Payload);
