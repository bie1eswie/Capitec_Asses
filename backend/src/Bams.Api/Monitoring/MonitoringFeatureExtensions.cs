using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Bams.Infrastructure;

namespace Bams.Api.Monitoring;

public static class MonitoringFeatureExtensions
{
    public static IServiceCollection AddBamsMonitoring(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<BamsDbContext>("postgres", tags: ["ready"]);

        return services;
    }

    public static WebApplication MapBamsMonitoring(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        }).AllowAnonymous();
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        }).AllowAnonymous();

        return app;
    }
}