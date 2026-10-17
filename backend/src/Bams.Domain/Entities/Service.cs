namespace Bams.Domain;

public sealed class Service
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

    public int DurationMinutes
    {
        get;
        set;
    }

    public bool IsActive
    {
        get;
        set;
    }
 = true;

    public List<BranchService> Branches
    {
        get;
        set;
    }
 = [];
}
