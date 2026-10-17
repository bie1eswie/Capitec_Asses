# Architecture and boundaries

```text
Angular customer ───┐
                    ├── HTTPS ── Bams.Api (controllers / middleware)
Angular admin ───────┘                 │
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

Security boundary: Customer endpoints are public read-only; admin portal **UI is not security**. No mutation endpoints until OIDC/OAuth2 authentication, role/branch scoping and integration tests are added.

Observability: `/health/live`, `/health/ready`; standard ASP.NET request logs; OpenTelemetry integration pending.
