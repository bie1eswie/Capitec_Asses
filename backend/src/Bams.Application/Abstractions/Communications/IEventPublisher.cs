namespace Bams.Application.Abstractions.Communications;

public interface IEventPublisher
{
    Task PublishAsync(EventEnvelope envelope, CancellationToken cancellationToken);
}
