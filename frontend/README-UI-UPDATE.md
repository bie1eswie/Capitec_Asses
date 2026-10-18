# BAMS Angular design refresh

Both Angular apps now use the shared green BAMS theme. Customer portal includes login, registration, password-reset request, branch discovery, service selection, date/time availability, booking review, confirmation, and My Appointments with cancellation. The admin app includes a staff role gate, dashboard, searchable branch directory, and styled placeholders for missing administration APIs.

## Authentication note
The existing API returns access and refresh tokens in the login response. This version retains both **only in memory**, attaches JWT access tokens to authenticated API requests, and deliberately requires sign-in again after a full browser reload. To provide persistent sessions securely, implement a server-side refresh-token cookie or an OIDC BFF pattern; do not put refresh tokens in localStorage.

## Integration limitations
- Booking payload now matches the current authenticated API (branchId, serviceId, startsAt only); guest tokens and customer-detail booking inputs were removed.
- Administrative write/report APIs do not exist in the uploaded code, so those panels are intentionally empty with clearly marked backend dependencies.
- Rescheduling requires a dedicated availability-to-reschedule flow; existing API endpoint is present but the new UI leaves rescheduling disabled until that flow is safely wired.
- Client-side role gating is for UX only. Server authorization remains mandatory.
- Angular template compilation still needs verification on a machine with dependencies installed.

## Run
From frontend: npm install; npm run start:customer; npm run start:admin. The development proxy targets the existing API.
