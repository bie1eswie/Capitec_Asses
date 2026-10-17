namespace Bams.Domain;

/// <summary>For half-open [start,end) intervals, enforce peak overlapping reservation count.</summary>
public static class CapacityPolicy
{
    public static bool HasCapacity(DateTimeOffset start, DateTimeOffset end, IEnumerable<(DateTimeOffset Start, DateTimeOffset End)> reservations, int capacity)
    {
        if (end <= start || capacity < 1)
        {
            return false;
        }
        var points = new List<(DateTimeOffset Time, int Delta)>
        {
            (start, 1), (end, -1)
        };
        foreach (var (s, e) in reservations)
        {
            if (s < end && e > start)
            {
                points.Add((s, 1));
                points.Add((e, -1));
            }
        }
        var concurrent = 0;
        foreach (var point in points.OrderBy(x => x.Time).ThenBy(x => x.Delta))
        {
            concurrent += point.Delta;
            if (concurrent > capacity)
            {
                return false;
            }
        }
        return true;
    }
}
