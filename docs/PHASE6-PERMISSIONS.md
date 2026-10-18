# Phase 6 — Permission-based Authorization (Safe Baseline)

## Implemented
- Server-owned permission catalogue and policy registration.
- AuthorizationHandler uses a trusted server-side resolver, **not a frontend-supplied permission list**.
- Login/refresh and `/accounts/session` now return permissions for Angular guards.
- The outbox replay endpoint requires `outbox.replay` permission.
- Removed duplicate IConfiguration parameter in AccountsController (compiler error).
- Removed creation of a separate service provider during database seeding.

## Deliberate restrictions
- SystemAdministrator receives listed permissions.
- BranchStaff and BranchManager receive **no branch-scoped permissions** until a verified staff/branch assignment model exists. This prevents unauthorized cross-branch access.
- Roles and permissions from the client are never trusted for backend authorization.
- Existing dashboard and branch directory remain role-gated, not branch-scoped; review before exposing sensitive per-branch information.
- No staff appointment, service management, notification listing or reporting endpoints were invented.

## Next implementation patch
1. Introduce StaffBranchAssignment entity, repository, EF mapping and additive migration.
2. Resolve assigned branch IDs for authenticated staff and implement resource-based branch authorization.
3. Return branch-scoped permission context in a versioned session response (and re-check every API request).
4. Implement protected admin business endpoints and verified tests.

## Validation
Static inspection and ZIP verification only. dotnet executable unavailable in this environment; no .NET build or integration test is claimed.
