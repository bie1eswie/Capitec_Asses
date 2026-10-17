namespace Bams.Application.Abstractions.Communications;

public interface IEventSubscriber
{
    Task<EventEnvelope?> ReceiveAsync(CancellationToken cancellationToken);

    Task AcknowledgeAsync(EventEnvelope envelope, CancellationToken cancellationToken);

    Task RejectAsync(EventEnvelope envelope, bool retry, string failureDetails, CancellationToken cancellationToken);
}
