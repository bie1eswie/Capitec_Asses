# Copilot Instructions

## Project Guidelines
- For Development, prefer the provided relaxed HS256 JWT setup with a symmetric config key; production should retain the stronger RSA/RS256 key-ring validation.
- For data seeding, control startup execution with a boolean setting in appsettings (e.g. Seed:Enabled), rather than always seeding or relying only on an explicit CLI command.