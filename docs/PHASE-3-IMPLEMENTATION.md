# Phase 3 — Secure Guest Access and Simulated Communications

## Included
- Guest appointment access through 256-bit random, SHA-256-hashed capability tokens issued on first successful booking. Grants expire after 30 days. Tokens are never stored in plaintext; token-retrieval is not supported. Duplicate idempotency responses do not re-issue a token.
- Appointment lookup, cancellation, and rescheduling require `X-Appointment-Token` and matching appointment ID. The booking request still requires `Idempotency-Key`.
- Transactional outbox processor runs as an ASP.NET Core hosted service every 5 seconds. Uses PostgreSQL row locks with `SKIP LOCKED`, at-most-five processing attempts, unique notification-per-event and simulated statuses. No actual external provider is used.
- Customer Angular portal supports booking confirmation and guest self-service management.

## Database setup
Fresh database: apply `001_initial.sql`, `002_seed.sql`, `003_booking_phase2.sql`, then `004_guest_access_and_notification_processing.sql`. For existing Phase 2 database apply only 004. Use one migration authority; scripts are the current authority.

## Run
Run `docker compose up -d db`, apply required SQL as documented, then `cd backend && dotnet run --project src/Bams.Api`. In another terminal `cd frontend && npm install && npm run start:customer`.

## Important boundaries / production blockers
1. **Simulated delivery only**: new users see a management token on-screen, not in email. If they lose it, they cannot manage the appointment until secure real-world verified token recovery is implemented. Do not expose tokens in logs, analytics, URLs, or browser-local storage.
2. Staff-facing UI is not included. Administrative data remains behind API authentication, permissions, branch scoping, and audit requirements.
3. Outbox polling runs in the API host for pilot deployment; move it to a separate worker process for independently scaled production deployments. It uses PostgreSQL transactions and `SKIP LOCKED` but does not yet use SQS.
4. Failed outbox events after five attempts require an operations replay/dead-letter procedure (not implemented). The error is logged at batch level, not per-event, to avoid PII leaks; add safe failure diagnostics.
5. Bookings remain subject to Phase 2 capacity and schedule logic. High-contention bookings, concurrent cancel/reschedule races, token handling, HTTP authorization, integration testing and security assessment are prerequisites for production release.
6. Current simulation rendering reads current appointment state, not the appointment snapshot at event occurrence; rapid successive events may render the current time. For audit-grade messaging introduce versioned event payload snapshots.
7. Booking idempotency keys currently have global scope (Phase 2). Production must scope by authenticated actor or guest session and provide an expiry/retention policy.
8. Cancellation/rescheduling currently do not enforce a minimum notice window beyond the reschedule target; define branch-specific policies before public launch.

## API
- `POST /api/v1/appointments` — responds with `managementToken` only on first successful creation.
- `GET /api/v1/appointments/{id}` — requires `X-Appointment-Token`.
- `POST /api/v1/appointments/{id}/cancel` — requires token.
- `POST /api/v1/appointments/{id}/reschedule` — requires token, body `{ "startsAt": "2026-11-20T08:30:00Z" }`.

All notification records use `SIMULATED` status and do not claim external delivery.
