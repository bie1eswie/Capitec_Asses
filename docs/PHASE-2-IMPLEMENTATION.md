# Phase 2 — Scheduling and booking implementation

## Delivered
- Branch weekday operating schedules and date-specific closures in separate domain files and EF configurations.
- Read-only availability endpoint (`GET /api/v1/availability?branchId=...&serviceId=...&date=2026-10-20`).
- `POST /api/v1/appointments` with `Idempotency-Key` and customer data; 201 on success, 409 for conflicts.
- Serializable-per-branch-service booking logic using a PostgreSQL row lock and peak overlap validation.
- Transactional creation of Customer, Appointment, BookingRequest, OutboxEvent and simulated NotificationRecord.
- Application use cases for cancellation/rescheduling, but **HTTP routes are deliberately disabled** until customer authorization is added.

## Important limits before production
- Booking endpoint does not yet enforce customer verification. Add anti-bot protection, rate limiting, abuse detection and verified contact workflow.
- `GET /appointments/{id}` and guest mutation APIs return HTTP 501 until secure guest access is delivered.
- Outbox is persisted but there is **no publisher/consumer**; notification preview is recorded in the booking transaction and is **never sent**.
- Business holidays, recurring exceptions, multi-interval daily schedules, staffing assignment, attendance, auditing and admin management are not yet exposed.
- Rebook/cancel use cases require authorization guards when wired to HTTP.
- Idempotency keys are global in this pilot; production should scope keys by authenticated caller and operation.
- Add EF migrations (current deployment uses SQL scripts), automated concurrency tests, and full compile/integration verification on a machine with .NET SDK.

## Run
1. `docker compose up -d db`
2. Apply `database/001_initial.sql`, `database/002_seed.sql`, then `database/003_booking_phase2.sql` to PostgreSQL (`psql`).
3. `cd backend && dotnet restore Bams.slnx && dotnet run --project src/Bams.Api`
4. `cd frontend && npm install && npm run start:customer`
5. Browse customer portal or Swagger (`http://localhost:5100/swagger`).

All timestamps returned by the API are UTC. The frontend converts them to Africa/Johannesburg for display.
