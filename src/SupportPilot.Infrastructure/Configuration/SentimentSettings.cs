namespace SupportPilot.Infrastructure.Configuration;

/// <summary>
/// Controls sentiment-based auto-escalation. A user message scoring at or
/// above EscalationThreshold triggers an automatic support ticket.
/// </summary>
public class SentimentSettings
{
    public float EscalationThreshold { get; set; } = 0.7f;
}