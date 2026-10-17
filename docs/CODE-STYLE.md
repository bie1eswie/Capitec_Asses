# C# Code Style — BAMS

- One top-level class, record, enum or interface per `.cs` file.
- Four-space indentation, file-scoped namespaces, and braces on separate lines.
- Explicit `if` blocks for business rules and exception handling.
- Repositories implement contracts defined by Application.
- Controller actions delegate to application services; no direct EF Core access.
- Prefer descriptive variables over single-character names.
- Avoid dense single-line use cases, nested ternaries and inline try/catch.
- Keep transactional logic in application services and unit-of-work contracts.
- Run `dotnet format Bams.slnx` from `backend/` after installing the .NET SDK.

This package focuses on readability and does not include a verified build or integration test run.
