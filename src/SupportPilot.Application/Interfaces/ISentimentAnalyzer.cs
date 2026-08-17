namespace SupportPilot.Application.Interfaces;

/// <summary>
/// Scores how frustrated a customer message sounds, from 0.0 (calm) to
/// 1.0 (extremely frustrated). Used to auto-escalate conversations without
/// relying on the agent's own tool-choice judgment.
/// </summary>
public interface ISentimentAnalyzer
{
    Task<float> AnalyzeAsync(string message);
}