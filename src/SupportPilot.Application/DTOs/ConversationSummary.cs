namespace SupportPilot.Application.DTOs;

/// <summary>
/// Lightweight projection for the conversations list — avoids loading full
/// message bodies for every row. Built via LINQ projection so EF translates
/// aggregation (count, last message) into SQL rather than loading everything
/// into memory first.
/// </summary>
public record ConversationSummary(
    Guid Id,
    DateTime CreatedAt,
    bool IsEscalated,
    Guid? LinkedTicketId,
    int MessageCount,
    string? LastMessagePreview);