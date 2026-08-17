namespace SupportPilot.Domain.Common;

/// <summary>
/// Response from any chat provider — assistant text plus token usage.
/// </summary>
public record ChatResponse(
    string Content,
    int InputTokens,
    int OutputTokens);