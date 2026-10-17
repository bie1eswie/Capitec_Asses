namespace Bams.Application.Abstractions.Communications;

public sealed class EventDispatcher(
    EventRegistry registry,
    IEnumerable<IEventHandlerInvoker> handlerInvokers) : IEventDispatcher
{
    private readonly IReadOnlyDictionary<(string EventName, int Version), IEventHandlerInvoker[]> handlers =
        handlerInvokers
            .GroupBy(invoker => (invoker.EventName, invoker.Version))
            .ToDictionary(group => group.Key, group => group.ToArray());

    public async Task DispatchAsync(EventEnvelope envelope, CancellationToken cancellationToken)
    {
        var descriptor = registry.GetRequired(envelope.EventName, envelope.Version);
        if (!handlers.TryGetValue((descriptor.EventName, descriptor.Version), out var eventHandlers))
        {
            return;
        }

        foreach (var handler in eventHandlers)
        {
            await handler.InvokeAsync(envelope.Payload, envelope, cancellationToken);
        }
    }
}
