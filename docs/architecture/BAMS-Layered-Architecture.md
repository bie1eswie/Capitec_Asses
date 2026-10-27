# BAMS | Layered Architecture

![BAMS Layered Architecture](./docs/images/bams-layered-architecture.png)

## Overview

This diagram shows the architecture and flow across the BAMS solution:

- **Presentation Layer** (Angular SPA)
- **API Layer** (Bams.Api)
- **Application Layer** (Bams.Application)
- **Domain Layer** (Bams.Domain)
- **Infrastructure Layer** (Bams.Infrastructure)
- **Data / External Systems** (PostgreSQL, Outbox table, Telemetry backend)

It also highlights:

- Repository and UnitOfWork interaction
- Outbox processing lifecycle (claim/publish/fail/replay)
- Worker telemetry and observability with OpenTelemetry
- Async boundary between request transaction and background side effects

## Notes

- Gray arrows: calls/dependencies
- Red arrows: asynchronous outbox path
- Dotted arrow: worker telemetry
