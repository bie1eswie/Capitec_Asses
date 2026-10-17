namespace Bams.Domain;

public sealed class BranchService
{
    public Guid BranchId
    {
        get;
        set;
    }

    public Branch Branch
    {
        get;
        set;
    }
 = null!;

    public Guid ServiceId
    {
        get;
        set;
    }

    public Service Service
    {
        get;
        set;
    }
 = null!;

    public int Capacity
    {
        get;
        set;
    }
 = 1;

    public bool IsEnabled
    {
        get;
        set;
    }
 = true;
}
