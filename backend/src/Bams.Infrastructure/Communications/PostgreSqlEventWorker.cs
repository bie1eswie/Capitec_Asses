using Bams.Application.Abstractions.Communications;
using Bams.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bams.Infrastructure;

public sealed class PostgreSqlEventWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<MessagingOptions> options,
    ILogger<PostgreSqlEventWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var didWork = await DispatchOneAsync(stoppingToken);
                if (!didWork)
                {
                    await Task.Delay(TimeSpan.FromSeconds(options.Value.PollIntervalSeconds), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "PostgreSQL event dispatch cycle failed.");
                await Task.Delay(TimeSpan.FromSeconds(options.Value.PollIntervalSeconds), stoppingToken);
            }
        }
    }

    private async Task<bool> DispatchOneAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var subscriber = scope.ServiceProvider.GetRequiredService<IEventSubscriber>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IOutboxDispatcher>();
        var envelope = await subscriber.ReceiveAsync(cancellationToken);
        if (envelope is null)
        {
            return false;
        }

        await dispatcher.DispatchAsync(envelope, cancellationToken);

        return true;
    }
}
