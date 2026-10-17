namespace Bams.Api.Contracts.Accounts;

public sealed record PasswordResetConfirmationRequest(string UserId, string Token, string NewPassword);
