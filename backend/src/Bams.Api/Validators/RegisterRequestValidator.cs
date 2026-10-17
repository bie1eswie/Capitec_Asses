using Bams.Api.Contracts.Accounts;
using FluentValidation;

namespace Bams.Api.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress();
        RuleFor(request => request.Password).NotEmpty().MinimumLength(12);
        RuleFor(request => request.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.LastName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Phone).MaximumLength(40);
    }
}
