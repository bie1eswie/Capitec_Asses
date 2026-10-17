namespace Bams.Application.Abstractions.Communications;

public interface IEventDispatcher
{
    Task DispatchAsync(EventEnvelope envelope, CancellationToken cancellationToken);
}
