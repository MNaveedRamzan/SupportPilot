namespace SupportPilot.Application.DTOs;

/// <summary>
/// Answer returned to the client, including retrieval metadata so the caller
/// can see whether the answer was grounded in the knowledge base.
/// </summary>
public record ChatAnswer(
    string Content,
    bool AnsweredFromKnowledgeBase,
    int RetrievedChunks,
    float TopScore,
    int InputTokens,
    int OutputTokens);