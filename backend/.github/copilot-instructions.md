# Copilot Instructions

## Project Guidelines
- For this BAMS repository, follow the user's coding standards: .NET 10 Clean Architecture (Domain <- Application <- Infrastructure, API as composition root); keep one type per file, readable multi-line C#, and place abstractions/repositories in Application with Infrastructure implementations. Preserve existing architecture, use a transactional database outbox for communications instead of replacing it with direct broker-only publication, and ensure booking idempotency.
- Use JWT Bearer for API authentication with secure rotating persisted refresh sessions, remove guest access, and avoid persisting Angular tokens in browser storage.