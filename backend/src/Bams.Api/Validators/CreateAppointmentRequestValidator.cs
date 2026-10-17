using Bams.Api.Contracts.Appointments;
using FluentValidation;

namespace Bams.Api.Validators;

public sealed class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentRequestValidator()
    {
        RuleFor(request => request.BranchId).NotEmpty();
        RuleFor(request => request.ServiceId).NotEmpty();
        RuleFor(request => request.StartsAt).NotEmpty();
    }
}
