using Bams.Api.Contracts.Appointments;
using FluentValidation;

namespace Bams.Api.Validators;

public sealed class RescheduleAppointmentRequestValidator : AbstractValidator<RescheduleAppointmentRequest>
{
    public RescheduleAppointmentRequestValidator()
    {
        RuleFor(request => request.StartsAt).NotEmpty();
    }
}
