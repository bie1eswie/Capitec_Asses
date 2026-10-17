using Bams.Api.Contracts.Accounts;
using FluentValidation;

namespace Bams.Api.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress();
        RuleFor(request => request.Password).NotEmpty();
    }
}
