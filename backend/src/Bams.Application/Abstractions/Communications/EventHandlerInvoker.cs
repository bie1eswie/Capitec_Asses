namespace Bams.Application.Abstractions.Communications;

public sealed class EventHandlerInvoker<TEvent>(
    string eventName,
    int version,
    IMessageSerializer serializer,
    IEnumerable<IEventHandler<TEvent>> handlers) : IEventHandlerInvoker
{
    private readonly IReadOnlyList<IEventHandler<TEvent>> eventHandlers = handlers.ToArray();

    public string EventName { get; } = eventName;

    public int Version { get; } = version;

    public async Task InvokeAsync(string payload, EventEnvelope envelope, CancellationToken cancellationToken)
    {
        var eventData = serializer.Deserialize<TEvent>(payload);
        foreach (var handler in eventHandlers)
        {
            await handler.HandleAsync(eventData, envelope, cancellationToken);
        }
    }
}
