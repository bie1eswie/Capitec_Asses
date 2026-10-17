namespace Bams.Application.Abstractions.Communications;

public interface IEventHandler<in TEvent>
{
    Task HandleAsync(TEvent eventData, EventEnvelope envelope, CancellationToken cancellationToken);
}
