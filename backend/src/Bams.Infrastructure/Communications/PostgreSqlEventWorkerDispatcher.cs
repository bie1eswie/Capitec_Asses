using Bams.Application.Abstractions.Communications;
using Bams.Application;
using Microsoft.Extensions.Logging;

namespace Bams.Infrastructure;

public sealed class PostgreSqlEventWorkerDispatcher(
    IEventSubscriber subscriber,
    IEventDispatcher eventDispatcher,
    ILogger<PostgreSqlEventWorkerDispatcher> logger) : IOutboxDispatcher
{
    public async Task DispatchAsync(EventEnvelope envelope, CancellationToken cancellationToken)
    {
        try
        {
            await eventDispatcher.DispatchAsync(envelope, cancellationToken);
            await subscriber.AcknowledgeAsync(envelope, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Event {EventId} dispatch failed.", envelope.EventId);
            await subscriber.RejectAsync(envelope, true, exception.ToString(), cancellationToken);
        }
    }
}
