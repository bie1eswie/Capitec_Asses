using Bams.Domain;
using Xunit;

namespace Bams.Domain.Tests;

public sealed class CapacityPolicyTests
{
    [Fact]
    public void RejectsOverlappingCapacityOne()
    {
        var t = new DateTimeOffset(2026, 10, 20, 9, 0, 0, TimeSpan.Zero);
        Assert.False(CapacityPolicy.HasCapacity(t, t.AddMinutes(30), [(t.AddMinutes(15), t.AddMinutes(45))], 1));
    }
    [Fact]
    public void AllowsAdjacentTimes()
    {
        var t = new DateTimeOffset(2026, 10, 20, 9, 0, 0, TimeSpan.Zero);
        Assert.True(CapacityPolicy.HasCapacity(t, t.AddMinutes(30), [(t.AddMinutes(30), t.AddMinutes(60))], 1));
    }
    [Fact]
    public void CalculatesPeakNotCountOfAllOverlaps()
    {
        var t = new DateTimeOffset(2026, 10, 20, 9, 0, 0, TimeSpan.Zero);
        Assert.True(CapacityPolicy.HasCapacity(t, t.AddMinutes(60), [(t, t.AddMinutes(30)), (t.AddMinutes(30), t.AddMinutes(60))], 2));
    }
}
