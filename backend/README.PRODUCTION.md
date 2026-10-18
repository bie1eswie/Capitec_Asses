# Production Readiness Status

Implemented and verified:

- Startup demo seeding is disabled by default. Seeding requires an explicit `Seed:Enabled=true` setting and externally supplied admin credentials.
- CI builds and runs tests without suppressing test failures.
- The API image workflow publishes to GHCR with `GITHUB_TOKEN`, least-privilege package permissions, and both `latest` and commit-SHA tags.
- API health endpoints are available at `/health/live` and `/health/ready`.
- Provider-neutral OpenTelemetry traces, metrics, and logs can be exported over OTLP when an OTLP endpoint is configured.
- Production JWT configuration requires an RSA key ring and RS256; the development HMAC key path is limited to Development.
- The current NuGet vulnerability audit reports no vulnerable packages, and the API Release publish succeeds.

Required before a production deployment:

- Supply `ConnectionStrings__Postgres`, JWT issuer/audience, active key ID, and RSA private/public key material through a managed secret store. Never commit production secrets.
- Configure `Cors__AllowedOrigins` for the deployed frontend and verify the deployed client URL and HTTPS settings.
- The API currently applies EF migrations during startup. For multi-replica deployments, move migration execution to a controlled release job before scaling out.
- Configure PostgreSQL for CI through `BAMS_TEST_POSTGRES`; six database integration tests currently skip when this is absent.
- Decide on a deliberate first-admin provisioning procedure. Keep `Seed__Enabled` false in production unless running a controlled seed operation with managed credentials.
- Configure the GitHub repository to allow publishing packages to GHCR, then deploy using the immutable commit-SHA image tag.
- Run the frontend production build and end-to-end checks against the actual API deployment.

Observability configuration:

- Development requires no collector endpoint; traces, metrics, and logs use console exporters automatically when OTLP endpoints are absent.
- Set `OTEL_SERVICE_NAME` (defaults to `Bams.Api`) and `OTEL_EXPORTER_OTLP_ENDPOINT` or signal-specific `OTEL_EXPORTER_OTLP_TRACES_ENDPOINT`, `OTEL_EXPORTER_OTLP_METRICS_ENDPOINT`, and `OTEL_EXPORTER_OTLP_LOGS_ENDPOINT`.
- Use standard `OTEL_EXPORTER_OTLP_PROTOCOL` and `OTEL_EXPORTER_OTLP_HEADERS` settings as needed; provide any credentials through the deployment secret store.
- For AWS, point OTLP to an AWS Distro for OpenTelemetry (ADOT) collector. For Azure Monitor, point OTLP to an OpenTelemetry Collector configured with the Azure Monitor exporter. No cloud-specific exporter package is required by BAMS.
