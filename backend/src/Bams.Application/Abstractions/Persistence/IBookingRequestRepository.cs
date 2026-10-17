using Bams.Domain;

namespace Bams.Application;

public interface IBookingRequestRepository
{
    Task<BookingRequest?> FindByIdempotencyKeyAsync(
        string key,
        string ownerUserId,
        string operation,
        CancellationToken cancellationToken);

    void Add(BookingRequest request);
}
