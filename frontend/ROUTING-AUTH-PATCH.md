# BAMS Angular router / guards patch

Changes:
- Angular Router configured for the customer app, with protected and public routes.
- Shared `authGuard` restores the HttpOnly refresh-cookie session before activation.
- Backend role and permission authorization remains mandatory for staff endpoints.
- Shared `guestGuard` redirects already-authenticated customers away from login/registration; this is not guest booking access.
- Bearer interceptor moved to a dedicated file and preserves single-flight refresh, protected logout request, and one retry on 401.
- Error interceptor provides a central place for future error normalization (currently pass-through).
- Existing app shells are retained via a router-outlet and lightweight route-view adapter so existing booking UI remains functional; full screen-level extraction is still pending.
- Tokens remain memory-only and refresh-cookie-driven.

Validation: verify Angular builds in an environment with npm dependencies installed; API and browser tests are not executed in this package.
