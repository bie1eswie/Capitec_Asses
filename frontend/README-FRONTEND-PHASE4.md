# BAMS Angular — Follow-up UI patch

## Implemented
- Extracted customer authentication forms into `AuthPanelComponent` with distinct template and typed outputs.
- Extracted appointment cards into `AppointmentListComponent`.
- Added date-and-time availability-based rescheduling using the existing authenticated endpoints.
- Added automatic single-flight access token rotation on HTTP 401 and exactly one retry; account endpoints bypass the interceptor to avoid recursion.
- Existing branch selection and booking views, and role-gated admin views remain intact.

## Known backend dependencies and limits
- Current `/api/v1/accounts/refresh` expects refresh credentials in JSON body; tokens therefore remain in memory only. **A page reload requires login**. Safe reload restoration requires a backend refresh-cookie or OAuth/OIDC BFF design; do not use localStorage for refresh tokens.
- The admin appointments, notifications, reports, schedules, and staff management endpoints remain unavailable, so these views remain honest placeholders.
- Rescheduling API is expected to accept `{ startsAt }` and to enforce user ownership and capacity atomically.
- The Angular dependency tree was not installed in the execution environment, so compile and browser tests must be run on a development machine.

## Run locally
`cd frontend && npm ci && npm run build:customer && npm run build:admin`
