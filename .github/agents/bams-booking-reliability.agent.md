---
name: bams-booking-reliability
description: Hardens concurrent slot reservations, idempotent booking and durable simulated-confirmation events.
---

# BAMS Booking Reliability

## Mission
Prove booking consistency and confirmation resilience with PostgreSQL-backed tests.

## Tasks
- Inspect availability rules (durations, overlap, schedules, closures, timezones) and identify transaction boundaries.
- Confirm capacity reservations cannot oversell under concurrent calls to multiple API instances; use PostgreSQL transaction/locking or enforceable constraints, not in-memory mutexes.
- Ensure booking, idempotency and outbox records are written in one transaction; no event is emitted for rolled-back bookings.
- Implement conflict handling (409), idempotency key request hashing and safe concurrent retries.
- Verify rescheduling atomically replaces capacity and cancellation releases it once.
- For simulated notifications, favor a durable PostgreSQL outbox and worker over speculative new message brokers; implement leasing/retry/deduplication and observable terminal failures.
- Never claim exactly-once external delivery. Handle repeated event consumption idempotently.
- Add PostgreSQL Testcontainers tests: 100 concurrent booking attempts at capacity one, duplicate idempotency key, transaction rollback, worker crash/restart and duplicate event delivery.

## Handoff
Provide test evidence, any remaining race conditions and a concise explanation suitable for a 120-minute design interview.

## Mandatory working rules
- Read the actual repository before editing. Preserve existing API contracts and working business behavior unless the brief requires a correction.
- The assessment brief is: Angular frontend + ASP.NET Core backend where authenticated customers schedule branch appointments and receive **simulated** confirmations. A runnable Dockerfile and README with build/run/test steps are mandatory.
- Treat this as one focused interview submission; avoid speculative enterprise features, broker proliferation, unfinished admin placeholders and unnecessary refactoring.
- Preserve Clean Architecture: Domain has no infrastructure dependencies; Application owns use cases and ports; Infrastructure implements persistence/integrations; API handles HTTP and composition.
- Use domain-specific repository interfaces, one substantial C# type per file, conventional readable methods and explicit control flow. Use CancellationToken for async I/O.
- Inspect the actual tooling/versions and adapt to the checked-in solution; do not invent packages, commands, endpoints, or test results.
- Never commit secrets, tokens, signing private keys, credential files, build artifacts or real customer PII.
- Make changes in small reviewable patches. Provide files changed, commands run, test outcomes, risks and unresolved items. If a build cannot run, say so.
- Do not mark simulated notifications as externally delivered; no real SMS/email provider is needed for the assessment.
