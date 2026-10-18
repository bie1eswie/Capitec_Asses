# BAMS: Branch Appointment Management System

BAMS is a full-stack appointment booking application for customers and branch operations staff. Customers can create accounts, browse branches and services, book and manage appointments, and receive simulated confirmation notifications. Staff access is protected by role and permission-based authorization.

## Stack

- ASP.NET Core 10 API with Clean Architecture boundaries across API, Application, Domain, and Infrastructure.
- PostgreSQL with EF Core migrations, transactional booking/idempotency handling, and a durable notification outbox.
- Angular 21 customer application, served as a static SPA behind unprivileged Nginx.
- Docker Compose for the complete local stack; GitHub Actions for backend CI and API image publishing.

## Run The Full Stack

Prerequisites: Docker Engine with the Compose plugin. From the repository root:

```sh
docker compose up --build
```

The API applies EF Core migrations at startup and seeds a demo branch, services, roles, and administrator. Open:

- Customer portal: [http://localhost:8080](http://localhost:8080)
- API readiness: [http://localhost:8080/health/ready](http://localhost:8080/health/ready)
- OpenAPI/Swagger (Development): [http://localhost:8080/swagger](http://localhost:8080/swagger)

The local demo administrator is `admin@bams.local` with password `BamsAdmin!234567`. These Compose defaults and the development JWT signing key are for local evaluation only; override them through environment variables for any shared environment:

```text
BAMS_SEED_ADMIN_EMAIL
BAMS_SEED_ADMIN_PASSWORD
BAMS_DEV_JWT_SIGNING_KEY
BAMS_POSTGRES_PASSWORD
```

The database is persisted in the `bams_pgdata` volume. To remove the local database and all demo data, run `docker compose down -v`.

## Run Without Docker

Prerequisites: .NET 10 SDK, Node.js 22.12 or newer, npm, and PostgreSQL 17.

1. Create a PostgreSQL database and configure `ConnectionStrings__Postgres` for the API.
1. From `backend/`, restore and run the API:

```sh
dotnet restore Bams.slnx
dotnet run --project src/Bams.Api
```

The API applies EF Core migrations on startup. For local development, set `Seed__Enabled=true`, `Seed__AdminEmail`, and `Seed__AdminPassword` to provision demo data and an administrator.

1. In another terminal, install dependencies and run the customer application:

```sh
cd frontend
npm ci
npm run start:customer
```

The development proxy forwards `/api` and `/health` to `http://localhost:5100`. Staff and administrator authorization remains enforced by the API; this submission does not include a staff-facing frontend.

## Tests And Builds

From `backend/`:

```sh
dotnet test Bams.slnx --configuration Release
dotnet build src/Bams.Api/Bams.Api.csproj --configuration Release
```

PostgreSQL integration tests require a test database connection in `BAMS_TEST_POSTGRES`. Without it, those tests are skipped. Do not point this variable at a database containing data you need to keep.

From `frontend/`:

```sh
npm ci
npm run build:customer
```

There are currently no Angular unit-test spec files. The `npm test` script is configured but reports that no tests were found; customer UI behavior is therefore verified through the production build and should gain focused unit and browser tests before production release.

Docker builds the customer Angular production bundle and API Release image. The frontend image runs as the unprivileged Nginx user.

## Design Notes

- Booking commands enforce customer authorization, capacity rules, idempotency, and transactional persistence.
- Refresh credentials are held in HttpOnly cookies; API mutations validate antiforgery tokens. Access tokens remain in browser memory.
- Appointment confirmations are simulated through an outbox-backed worker; no external message is claimed to have been delivered.
- Admin authorization is enforced by API policies, not by hiding UI controls.
- API liveness and database readiness are exposed separately at `/health/live` and `/health/ready`.
- Backend observability uses OpenTelemetry with console export in Development and provider-neutral OTLP configuration for collectors. Frontend monitoring records structured startup and exception events in the browser console.

See [architecture](docs/ARCHITECTURE.md), [production operations](backend/README.PRODUCTION.md), [booking implementation](docs/PHASE-2-IMPLEMENTATION.md), and [notification processing](docs/PHASE-3-IMPLEMENTATION.md) for more detail.
