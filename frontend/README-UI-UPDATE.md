# BAMS Angular design refresh

The Angular customer portal uses the shared BAMS theme and includes login, registration, password-reset request, branch discovery, service selection, date/time availability, booking review, confirmation, and My Appointments with cancellation. No admin frontend is included; staff authorization is enforced by the backend API.

## Authentication note
The existing API returns access and refresh tokens in the login response. This version retains both **only in memory**, attaches JWT access tokens to authenticated API requests, and deliberately requires sign-in again after a full browser reload. To provide persistent sessions securely, implement a server-side refresh-token cookie or an OIDC BFF pattern; do not put refresh tokens in localStorage.

## Integration limitations
- Booking payload now matches the current authenticated API (branchId, serviceId, startsAt only); guest tokens and customer-detail booking inputs were removed.
- Rescheduling requires a dedicated availability-to-reschedule flow; existing API endpoint is present but the new UI leaves rescheduling disabled until that flow is safely wired.
- Client-side role gating is for UX only. Server authorization remains mandatory.
- Angular template compilation still needs verification on a machine with dependencies installed.

## Run
From frontend: npm install; npm run start:customer. The development proxy targets the existing API.
