namespace Bams.Application.Abstractions.Communications;

public interface IOutboxDispatcher
{
    Task DispatchAsync(EventEnvelope envelope, CancellationToken cancellationToken);
}
