# Architecture and boundaries

```text
Angular customer ───── HTTPS ── Bams.Api (controllers / middleware)
                                       │
                                Bams.Application (queries, DTOs, interfaces)
                                       │
                                Bams.Domain (entities, invariants)
                                       ▲
                                Bams.Infrastructure (EF Core, PostgreSQL, external adapters)
                                       │
                                  PostgreSQL
```

Dependencies: API → Application + Infrastructure; Infrastructure → Application + Domain; Application → Domain; Domain → none.

Phase 1 uses read-only queries: `IBranchReader` and `IServiceReader` are abstractions defined in Application and implemented in Infrastructure.

Phase 2 introduces booking aggregate behavior, transactional capacity, idempotency and outbox publication. Notification provider should use `ICommunicationProvider`, initially simulated; only process confirmed outbox events after the database transaction commits.

Security boundary: customer mutations are authenticated and protected by API authorization. Staff and administrator authorization policies remain on the API; this repository does not include a staff-facing frontend.

Monitoring feature: `Bams.Api.Monitoring.MonitoringFeatureExtensions` owns health-check registration and endpoint mapping. `/health/live` reports process liveness without dependency probes; `/health/ready` runs checks tagged `ready` (currently PostgreSQL).

Observability feature: `Bams.Api.Observability.ObservabilityFeatureExtensions` owns OpenTelemetry resource metadata, ASP.NET Core and outbound HTTP tracing, ASP.NET Core/runtime metrics, and log export. Development uses console exporters automatically when no OTLP endpoint is configured. Outside Development, signals are exported using OTLP only when the corresponding standard `OTEL_EXPORTER_OTLP_*_ENDPOINT` or shared `OTEL_EXPORTER_OTLP_ENDPOINT` is configured. Point OTLP at a collector or gateway to select a backend: AWS ADOT Collector can receive OTLP and export to AWS services; an OpenTelemetry Collector with the Azure Monitor exporter can receive OTLP and export to Azure Monitor. Provider-specific exporters belong in this composition boundary, not in application/domain code.
