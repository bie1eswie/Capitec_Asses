# Authentication remediation — October 2026

- API remains JWT Bearer authenticated; Identity application cookies are not enabled.
- Login now sets the opaque refresh credential in Secure, HttpOnly, SameSite=Strict cookie scoped to /api/v1/accounts.
- Refresh requires X-CSRF-TOKEN issued by GET /accounts/csrf-token and rotates the cookie.
- Logout requires JWT authorization and the CSRF token; it revokes the refresh credential before removing the cookie.
- Access tokens remain in Angular memory; restoreSession calls refresh on reload.
- Identity roles: Admin endpoint requires SystemAdministrator policy.
- JWT session uses sub and email claim names.
- Account action helpers added to Angular AuthApi; user-facing verify/reset routes still need UI integration.

## Deployment constraints

Frontend and backend must be served under the same site, ideally through a reverse proxy, with HTTPS. Strict refresh cookie will not work in arbitrary cross-site setups. The Angular proxy should forward Set-Cookie. CSRF token endpoint must not be cached.

## Migration/security notes

Existing refresh tokens returned to browsers in JSON must be revoked during rollout (revoke all old refresh sessions). Users must log in again. The pre-existing PEM signing key files have been excluded from this deliverable; configure external signing keys via secrets before starting the API. Configure RefreshTokenLifetimeDays consistently in configuration; currently defaults to 30 days in controller.

## Verification

No .NET SDK or installed Angular compiler available. Builds, unit/integration tests, and browser cookie/CSRF tests remain outstanding.
