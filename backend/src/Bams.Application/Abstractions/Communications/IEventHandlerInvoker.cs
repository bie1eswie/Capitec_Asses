namespace Bams.Application.Abstractions.Communications;

public interface IEventHandlerInvoker
{
    string EventName { get; }

    int Version { get; }

    Task InvokeAsync(string payload, EventEnvelope envelope, CancellationToken cancellationToken);
}
