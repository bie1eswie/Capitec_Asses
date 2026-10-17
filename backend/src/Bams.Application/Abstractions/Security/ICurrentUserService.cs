namespace Bams.Application.Abstractions.Security;

public interface ICurrentUserService
{
    string? UserId { get; }
}
