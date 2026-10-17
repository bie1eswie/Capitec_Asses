using System;
using Bams.Application;
using Xunit;

namespace Bams.Domain.Tests;

public class IdempotencyProviderTests
{
    [Fact]
    public void ComputeHash_Is_Deterministic_And_Scoped_To_Account()
    {
        var provider = new IdempotencyProvider();
        var r1 = new CreateAppointmentCommand
        {
            BranchId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            StartsAt = DateTimeOffset.UtcNow.AddDays(1).ToUniversalTime()
        };
        var r2 = new CreateAppointmentCommand
        {
            BranchId = r1.BranchId,
            ServiceId = r1.ServiceId,
            StartsAt = r1.StartsAt
        };

        var h1 = provider.ComputeHash(r1, "account-1");
        var h2 = provider.ComputeHash(r2, "account-1");
        Assert.Equal(h1, h2);
        Assert.NotEqual(h1, provider.ComputeHash(r2, "account-2"));
    }

    [Fact]
    public void ValidateIdempotencyKey_Throws_For_Invalid()
    {
        var validator = new BookingValidator();
        Assert.Throws<BookingException>(() => validator.ValidateIdempotencyKey(""));
        var longKey = new string('a', 161);
        Assert.Throws<BookingException>(() => validator.ValidateIdempotencyKey(longKey));
    }

    [Fact]
    public void ValidateStartsAtIsUtc_Throws_For_NonUtc()
    {
        var validator = new BookingValidator();
        var nonUtc = DateTimeOffset.Now; // local offset
        Assert.Throws<BookingException>(() => validator.ValidateStartsAtIsUtc(nonUtc));
    }
}
