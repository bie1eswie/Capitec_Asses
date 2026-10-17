namespace Bams.Domain;

public sealed class BranchSchedule
{
    public Guid Id
    {
        get;
        set;
    }

    public Guid BranchId
    {
        get;
        set;
    }

    public int DayOfWeek
    {
        get;
        set;
    }

    public TimeOnly OpensAt
    {
        get;
        set;
    }

    public TimeOnly ClosesAt
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
}
