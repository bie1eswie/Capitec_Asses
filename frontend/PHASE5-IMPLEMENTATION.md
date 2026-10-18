# BAMS Phase 5 — Frontend component and permissions patch

## Implemented
- Extracted customer home and booking confirmation into independent Angular standalone components with external HTML templates.
- Customer routes use authentication and guest guards; staff authorization is enforced by the backend.
- Preserved existing JWT auth interceptors and appointment API contracts.

## Required backend follow-up
The backend issues and validates staff permissions. This repository no longer includes an admin frontend; any future staff client should enforce these permissions in addition to backend policies.

## Remaining work
- Customer booking wizard still has substantial orchestration in its root component.
- Router outlet is still an adapter rather than full feature routing for every page.
- Staff appointment/notification/report APIs must be implemented before creating a staff client.
- Run npm install, Angular build, and browser checks in a dependency-enabled environment.

## Validation
The archive contains source only. No Angular compilation or E2E test success is claimed.
