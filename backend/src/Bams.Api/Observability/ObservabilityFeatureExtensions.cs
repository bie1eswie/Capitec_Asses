using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Bams.Api.Observability;

public static class ObservabilityFeatureExtensions
{
    public static IServiceCollection AddBamsObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var tracesEnabled = HasOtlpEndpoint(configuration, "TRACES");
        var metricsEnabled = HasOtlpEndpoint(configuration, "METRICS");
        var logsEnabled = HasOtlpEndpoint(configuration, "LOGS");
        var useDevelopmentConsole = environment.IsDevelopment()
            && !tracesEnabled
            && !metricsEnabled
            && !logsEnabled;

        if (!tracesEnabled && !metricsEnabled && !logsEnabled && !useDevelopmentConsole)
        {
            return services;
        }

        var serviceName = configuration["OTEL_SERVICE_NAME"]
            ?? configuration["Observability:ServiceName"]
            ?? "Bams.Api";

        var telemetry = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName)
                .AddAttributes([new KeyValuePair<string, object>("deployment.environment.name", environment.EnvironmentName)]));

        if (tracesEnabled || useDevelopmentConsole)
        {
            telemetry.WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation();
                if (tracesEnabled)
                {
                    tracing.AddOtlpExporter();
                }
                else
                {
                    tracing.AddConsoleExporter();
                }
            });
        }

        if (metricsEnabled || useDevelopmentConsole)
        {
            telemetry.WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
                if (metricsEnabled)
                {
                    metrics.AddOtlpExporter();
                }
                else
                {
                    metrics.AddConsoleExporter();
                }
            });
        }

        if (logsEnabled || useDevelopmentConsole)
        {
            telemetry.WithLogging(logging =>
            {
                if (logsEnabled)
                {
                    logging.AddOtlpExporter();
                }
                else
                {
                    logging.AddConsoleExporter();
                }
            });
        }

        return services;
    }

    private static bool HasOtlpEndpoint(IConfiguration configuration, string signal) =>
        !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"])
        || !string.IsNullOrWhiteSpace(configuration[$"OTEL_EXPORTER_OTLP_{signal}_ENDPOINT"]);
}