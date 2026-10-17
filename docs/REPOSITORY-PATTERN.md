# Repository Pattern and File Organization

- One public domain entity, enum, DTO, interface, service, and EF configuration per file.
- Domain has no ASP.NET, EF Core or messaging references.
- Application owns `IBranchRepository` and `IServiceRepository` ports and query orchestration.
- Infrastructure implements repository ports using scoped EF Core `BamsDbContext`.
- API controllers depend on `IBranchQueries` and `IServiceQueries`, **never** on `BamsDbContext`.
- `BamsDbContext` uses `ApplyConfigurationsFromAssembly` so each table mapping has its own configuration class.
- Repositories use `AsNoTracking()` for read-only operations and push filtering to PostgreSQL.
- Keep query paging bounded. The current public branches list has a 100-record cap; add cursor paging when needed.
- For future booking writes, define `IAppointmentRepository` and `IUnitOfWork` in Application; **use one EF transaction** for capacity, appointment, idempotency record and outbox. Do not commit separately in repositories.
- Repositories are persistence abstractions, not generic CRUD wrappers. Prefer domain-specific methods, avoiding unnecessary `IGenericRepository<T>`.
- Frontend files were left unchanged by this backend refactor.

## Execution status
Static inspection only; .NET SDK is not present in the authoring environment. Run `dotnet restore Bams.slnx && dotnet test Bams.slnx` before deploying.
