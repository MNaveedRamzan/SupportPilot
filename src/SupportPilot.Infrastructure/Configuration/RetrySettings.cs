namespace SupportPilot.Infrastructure.Configuration;

/// <summary>
/// Exponential backoff retry policy for transient AI provider failures.
/// </summary>
public class RetrySettings
{
    public int MaxAttempts { get; set; } = 3;
    public int InitialDelayMs { get; set; } = 1000;
    public int BackoffMultiplier { get; set; } = 2;
}