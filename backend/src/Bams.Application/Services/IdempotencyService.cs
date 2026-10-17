using System;
using Bams.Domain;

namespace Bams.Application;

public sealed class IdempotencyService(
    IBookingRequestRepository bookingRequests,
    IAppointmentRepository appointments,
    IdempotencyProvider idempotencyProvider)
{
    public string ComputeRequestHash(CreateAppointmentCommand command, string ownerUserId)
    {
        return idempotencyProvider.ComputeHash(command, ownerUserId);
    }

    public async Task<BookingResultDto?> FindReplayAsync(
        string key,
        string requestHash,
        string ownerUserId,
        string operation,
        CancellationToken cancellationToken)
    {
        var previousRequest = await bookingRequests.FindByIdempotencyKeyAsync(
            key,
            ownerUserId,
            operation,
            cancellationToken);
        if (previousRequest is null)
        {
            return null;
        }

        if (!string.Equals(previousRequest.RequestHash, requestHash, StringComparison.Ordinal))
        {
            throw new BookingException("Idempotency key has been used for different details.", 409);
        }

        var appointment = await appointments.FindAsync(previousRequest.AppointmentId, cancellationToken);
        if (appointment is null)
        {
            throw new BookingException("Booking not found.", 409);
        }

        return new BookingResultDto(
            appointment.Id,
            appointment.Reference,
            appointment.Status.ToString(),
            appointment.BranchId,
            appointment.ServiceId,
            appointment.StartsAt,
            appointment.EndsAt,
            true);
    }
}
