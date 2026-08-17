using SupportPilot.Domain.Common;
using Xunit;

namespace SupportPilot.Tests;

public class EscalationPolicyTests
{
    [Fact]
    public void ShouldEscalate_ScoreAboveThresholdAndNotAlreadyEscalated_ReturnsTrue()
    {
        bool result = EscalationPolicy.ShouldEscalate(
            alreadyEscalated: false, sentimentScore: 0.9f, threshold: 0.7f);

        Assert.True(result);
    }

    [Fact]
    public void ShouldEscalate_ScoreBelowThreshold_ReturnsFalse()
    {
        bool result = EscalationPolicy.ShouldEscalate(
            alreadyEscalated: false, sentimentScore: 0.5f, threshold: 0.7f);

        Assert.False(result);
    }

    [Fact]
    public void ShouldEscalate_AlreadyEscalated_ReturnsFalseEvenIfScoreIsHigh()
    {
        // A conversation that already has a ticket should never create a
        // second one, no matter how frustrated the next message sounds.
        bool result = EscalationPolicy.ShouldEscalate(
            alreadyEscalated: true, sentimentScore: 1.0f, threshold: 0.7f);

        Assert.False(result);
    }

    [Fact]
    public void ShouldEscalate_ScoreExactlyAtThreshold_ReturnsTrue()
    {
        // Boundary case: threshold comparison uses >=, so a score equal to
        // the threshold should still trigger escalation.
        bool result = EscalationPolicy.ShouldEscalate(
            alreadyEscalated: false, sentimentScore: 0.7f, threshold: 0.7f);

        Assert.True(result);
    }
}