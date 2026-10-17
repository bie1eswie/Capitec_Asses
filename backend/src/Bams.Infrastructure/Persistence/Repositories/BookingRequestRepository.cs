using Bams.Application;
using Bams.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bams.Infrastructure;

public sealed class BookingRequestRepository(BamsDbContext db) : IBookingRequestRepository
{
    public Task<BookingRequest?> FindByIdempotencyKeyAsync(
        string key,
        string ownerUserId,
        string operation,
        CancellationToken cancellationToken) =>
        db.BookingRequests.AsNoTracking()
            .FirstOrDefaultAsync(
                request => request.IdempotencyKey == key &&
                            request.OwnerUserId == ownerUserId &&
                            request.Operation == operation,
                cancellationToken);

    public void Add(BookingRequest request) => db.BookingRequests.Add(request);
}
