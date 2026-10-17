using Bams.Domain;

namespace Bams.Application;

public sealed class BookingValidator
{
    public void ValidateOwner(string ownerUserId)
    {
        if (string.IsNullOrWhiteSpace(ownerUserId))
        {
            throw new BookingException("An authenticated account is required to book an appointment.", 401);
        }
    }

    public void ValidateIdempotencyKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > 160)
        {
            throw new BookingException("A valid Idempotency-Key header is required.", 400);
        }
    }

    public void ValidateStartsAtIsUtc(DateTimeOffset startsAt)
    {
        if (startsAt.Offset != TimeSpan.Zero)
        {
            throw new BookingException("startsAt must be UTC (Z).", 400);
        }
    }

    public void ValidateBranchAndService(Branch? branch, Service? service, BranchService? branchService)
    {
        if (branch is null || service is null || branchService is null ||
            !branchService.IsEnabled || !service.IsActive || branch.Status != BranchStatus.Active)
        {
            throw new BookingException("Branch or service is unavailable.", 404);
        }
    }

    public void ValidateBookingTime(DateTimeOffset startsAt, int durationMinutes)
    {
        var now = DateTimeOffset.UtcNow;
        if (startsAt < now.AddMinutes(30) || startsAt > now.AddDays(60) ||
            startsAt.Minute % 15 != 0 || startsAt.Second != 0 || durationMinutes <= 0)
        {
            throw new BookingException("Booking must be 30 minutes to 60 days ahead and aligned to a 15 minute interval.", 400);
        }
    }
}
