namespace Bams.Application.DTOs;

public sealed record CustomerRegistration(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Phone);
