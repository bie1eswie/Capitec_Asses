namespace Bams.Domain;

public sealed class Branch
{
    public Guid Id
    {
        get;
        set;
    }

    public required string Code
    {
        get;
        set;
    }

    public required string Name
    {
        get;
        set;
    }

    public required string City
    {
        get;
        set;
    }

    public required string Address
    {
        get;
        set;
    }

    public string TimeZone
    {
        get;
        set;
    }
    = "Africa/Johannesburg";

    public BranchStatus Status
    {
        get;
        set;
    }
 = BranchStatus.Active;

    public List<BranchService> Services
    {
        get;
        set;
    }
 = [];
}
