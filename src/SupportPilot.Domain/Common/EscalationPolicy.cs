namespace SupportPilot.Domain.Common;

/// <summary>
/// Pure decision rule for sentiment-based auto-escalation. Kept dependency-free
/// (no I/O, no repositories) so it can be unit tested without mocking, and so
/// the escalation threshold logic isn't buried inside ChatHub's orchestration.
/// </summary>
public static class EscalationPolicy
{
    /// <summary>
    /// Returns true if a conversation should be escalated to a support ticket,
    /// given its current escalation state and the latest message's sentiment
    /// score. A conversation that's already escalated is never escalated again.
    /// </summary>
    public static bool ShouldEscalate(bool alreadyEscalated, float sentimentScore, float threshold)
    {
        if (alreadyEscalated)
        {
            return false;
        }

        return sentimentScore >= threshold;
    }
}