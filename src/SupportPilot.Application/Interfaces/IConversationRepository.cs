using SupportPilot.Application.DTOs;
using SupportPilot.Domain.Entities;
using SupportPilot.Domain.Enums;

namespace SupportPilot.Application.Interfaces;

public interface IConversationRepository
{
    Task<Conversation> CreateAsync();
    Task<Conversation?> GetByIdAsync(Guid conversationId);
    Task<Message> AddMessageAsync(Guid conversationId, ChatRole role, string content, float? sentimentScore = null);
    Task UpdateMessageSentimentAsync(Guid messageId, float score);
    Task MarkEscalatedAsync(Guid conversationId, Guid ticketId);

    /// <summary>
    /// Returns a page of conversation summaries, newest first, plus the
    /// total count across all conversations (for pagination UI).
    /// </summary>
    Task<PagedResult<ConversationSummary>> GetPagedSummariesAsync(int page, int pageSize);

    Task<int> GetTotalCountAsync();
    Task<int> GetEscalatedCountAsync();

    /// <summary>
    /// Average sentiment score across all scored (non-null) user messages.
    /// Returns null if no messages have been scored yet.
    /// </summary>
    Task<double?> GetAverageSentimentScoreAsync();

    Task<IReadOnlyList<ConversationAnalyticsRow>> GetConversationsForAnalyticsAsync(DateTime sinceUtc);
}