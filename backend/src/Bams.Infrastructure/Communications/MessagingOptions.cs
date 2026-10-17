namespace Bams.Infrastructure;

public sealed class MessagingOptions
{
    public const string SectionName = "Messaging";

    public int PollIntervalSeconds { get; set; } = 5;
    public int BatchSize { get; set; } = 25;
    public int MaximumAttempts { get; set; } = 10;
    public int LeaseDurationSeconds { get; set; } = 120;
}
