namespace Bams.Api.Contracts.Accounts;

public sealed class RegisterRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Phone { get; init; }
}
