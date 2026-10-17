using Bams.Domain;
using Xunit;

namespace Bams.Domain.Tests;

public class DomainTests
{
    [Fact]
    public void NewBranchDefaultsToActive()
    {
        var branch = new Branch
        {
            Code = "TEST",
            Name = "Test",
            City = "Johannesburg",
            Address = "Example"
        };
        Assert.Equal(BranchStatus.Active, branch.Status);
        Assert.Equal("Africa/Johannesburg", branch.TimeZone);
    }
}
