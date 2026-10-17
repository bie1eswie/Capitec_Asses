Production readiness notes

- CI: .github/workflows/ci.yml included (build, test, publish artifact).
- Docker: src/Bams.Api/Dockerfile provided (multi-stage build, runtime image based on aspnet:10.0).
- Health: API exposes /health/live and /health/ready endpoints.
- Nullable: solution enables nullable reference types via Directory.Build.props.

Recommended next steps:
- Configure secrets and connection strings in your CI provider (or use Key Vault).
- Add container image publish step (registry credentials) to CI for releasing images.
- Enable code analyzers or SonarCloud in CI for deeper quality checks.
