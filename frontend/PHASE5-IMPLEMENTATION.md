# BAMS Phase 5 — Frontend component and permissions patch

## Implemented
- Extracted customer home and booking confirmation into independent Angular standalone components with external HTML templates.
- Extracted unavailable admin feature messaging into its own standalone component.
- Added fail-closed `permissionGuard` for sensitive staff feature routes.
- Split admin route requirements: staff role for dashboard/directory, explicit permissions for other routes.
- Hide navigation links for routes without server-issued permissions.
- Preserved existing JWT auth interceptors and appointment API contracts.

## Required backend follow-up
The login/refresh responses currently do not prove permission claims are issued. Until the backend returns trusted `permissions` values, sensitive admin routes are intentionally inaccessible, even to staff roles. Implement permission issuance and matching API policies before enabling them.

## Remaining work
- Customer booking wizard and admin shell still have substantial orchestration in root components.
- Router outlet is still an adapter rather than full feature routing for every page.
- Real staff appointment/notification/report APIs must be implemented before replacing placeholders.
- Run npm install, Angular build, and browser checks in a dependency-enabled environment.

## Validation
The archive contains source only. No Angular compilation or E2E test success is claimed.
