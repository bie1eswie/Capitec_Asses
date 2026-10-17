using Bams.Domain;
using Bams.Application.Abstractions.Communications;
using Bams.Application.Abstractions.Security;
using Microsoft.Extensions.Logging;

namespace Bams.Application;

public sealed class CreateAppointmentUseCase(
    IAppointmentRepository appointments,
    IUnitOfWork unitOfWork,
    IBranchRepository branches,
    IServiceRepository services,
    IBookingRequestRepository bookingRequests,
    ICustomerRepository customers,
    ICurrentUserService currentUser,
    IEventPublisher eventPublisher,
    BookingValidator validator,
    AvailabilityService availability,
    IdempotencyService idempotency,
    AppointmentFactory appointmentFactory,
    OutboxFactory outboxFactory,
    ILogger<CreateAppointmentUseCase> logger)
{
    private const string Operation = "create-appointment";

    public async Task<BookingResultDto> ExecuteAsync(
        CreateAppointmentCommand command,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var ownerUserId = currentUser.UserId ?? string.Empty;
        validator.ValidateOwner(ownerUserId);
        validator.ValidateIdempotencyKey(idempotencyKey);
        validator.ValidateStartsAtIsUtc(command.StartsAt);

        var requestHash = idempotency.ComputeRequestHash(command, ownerUserId);
        logger.LogInformation(
            "Booking request for branch {BranchId} service {ServiceId} starts at {StartsAt}",
            command.BranchId,
            command.ServiceId,
            command.StartsAt);

        var result = await unitOfWork.ExecuteLockedAsync(
            command.BranchId,
            command.ServiceId,
            token => ExecuteWithinTransactionAsync(command, idempotencyKey, requestHash, ownerUserId, token),
            cancellationToken,
            $"{Operation}:{ownerUserId}:{idempotencyKey}");

        logger.LogInformation("Booking created {AppointmentId}", result.Id);
        return result;
    }

    private async Task<BookingResultDto> ExecuteWithinTransactionAsync(
        CreateAppointmentCommand command,
        string idempotencyKey,
        string requestHash,
        string ownerUserId,
        CancellationToken cancellationToken)
    {
        var customer = await customers.FindByIdentityUserIdAsync(ownerUserId, cancellationToken)
            ?? throw new BookingException("Customer profile is not linked to this account.", 403);
        var replay = await idempotency.FindReplayAsync(
            idempotencyKey,
            requestHash,
            ownerUserId,
            Operation,
            cancellationToken);
        if (replay is not null)
        {
            return replay;
        }

        var branch = await branches.FindActiveByIdAsync(command.BranchId, cancellationToken);
        var service = await services.FindActiveByIdAsync(command.ServiceId, cancellationToken);
        var branchService = await branches.FindBranchServiceAsync(command.BranchId, command.ServiceId, cancellationToken);
        validator.ValidateBranchAndService(branch, service, branchService);
        validator.ValidateBookingTime(command.StartsAt, service!.DurationMinutes);

        var endsAt = command.StartsAt.AddMinutes(service.DurationMinutes);
        await availability.EnsureAvailableAsync(
            command.BranchId,
            command.ServiceId,
            command.StartsAt,
            endsAt,
            branch!.TimeZone,
            branchService!.Capacity,
            null,
            cancellationToken);

        var appointment = appointmentFactory.Create(customer, command, service.DurationMinutes);
        appointment.OwnerUserId = ownerUserId;
        var now = DateTimeOffset.UtcNow;
        var bookingRequest = CreateBookingRequest(idempotencyKey, requestHash, ownerUserId, Operation, appointment.Id, now);
        var confirmationEvent = outboxFactory.CreateAppointmentConfirmed(appointment);

        appointments.Add(appointment);
        bookingRequests.Add(bookingRequest);
        await eventPublisher.PublishAsync(confirmationEvent, cancellationToken);

        return ToBookingResult(appointment, false);
    }

    private static BookingRequest CreateBookingRequest(
        string idempotencyKey,
        string requestHash,
        string ownerUserId,
        string operation,
        Guid appointmentId,
        DateTimeOffset now)
    {
        return new BookingRequest
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotencyKey,
            Operation = operation,
            RequestHash = requestHash,
            AppointmentId = appointmentId,
            OwnerUserId = ownerUserId,
            CreatedAt = now
        };
    }

    private static BookingResultDto ToBookingResult(Appointment appointment, bool isReplay)
    {
        return new BookingResultDto(
            appointment.Id,
            appointment.Reference,
            appointment.Status.ToString(),
            appointment.BranchId,
            appointment.ServiceId,
            appointment.StartsAt,
            appointment.EndsAt,
            isReplay);
    }
}
