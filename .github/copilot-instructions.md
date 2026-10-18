# BAMS Repository Instructions for GitHub Copilot

The sole purpose of the **Appointment Booking System**: Angular frontend + .NET API; logged-in customers select branch/service/slot and get a clearly **simulated** confirmation. Include a runnable Dockerfile and README with build, run and test instructions.

- Prioritize a working, reproducible, tested customer booking journey over speculative features.
- Preserve the current project, API contracts, Clean Architecture, Domain/Application/Infrastructure/API dependency rule and domain-specific Repository Pattern.
- One substantial C# type per file; readable methods with braces; thin controllers; async CancellationToken; typed DTOs and explicit validation.
- PostgreSQL owns booking integrity. Use atomic transactions, idempotency and durable outbox. Do not call external messaging inside booking transaction.
- ASP.NET Core Identity + JWT bearer. No guest appointment tokens. Verify ownership and branch scope on the server. Keep browser refresh credentials out of JavaScript-readable persistent storage.
- Never claim actual SMS/email was sent: all messages are simulated.
- Avoid hard-coded secrets, committed signing keys, node_modules, bin/obj or customer PII.
- Use xUnit + PostgreSQL integration tests and Angular tests. Do not report unrun tests as passing.
- Any patch must list affected files, verification commands/results, potential regressions and limitations.
