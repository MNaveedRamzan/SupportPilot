using Microsoft.EntityFrameworkCore;
using SupportPilot.Application.DTOs;
using SupportPilot.Application.Interfaces;
using SupportPilot.Domain.Entities;
using SupportPilot.Domain.Enums;
using SupportPilot.Infrastructure.Persistence;

namespace SupportPilot.Infrastructure.Conversations;

public class EfConversationRepository : IConversationRepository
{
    private const int PreviewMaxLength = 100;

    private readonly SupportPilotDbContext _dbContext;

    public EfConversationRepository(SupportPilotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Conversation> CreateAsync()
    {
        var conversation = new Conversation();
        _dbContext.Conversations.Add(conversation);
        await _dbContext.SaveChangesAsync();
        return conversation;
    }

    public async Task<Conversation?> GetByIdAsync(Guid conversationId)
    {
        return await _dbContext.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == conversationId);
    }

    public async Task<Message> AddMessageAsync(
        Guid conversationId, ChatRole role, string content, float? sentimentScore = null)
    {
        var message = new Message
        {
            ConversationId = conversationId,
            Role = role,
            Content = content,
            SentimentScore = sentimentScore
        };

        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync();
        return message;
    }

    public async Task UpdateMessageSentimentAsync(Guid messageId, float score)
    {
        var message = await _dbContext.Messages.FindAsync(messageId);
        if (message is null) return;

        message.SentimentScore = score;
        await _dbContext.SaveChangesAsync();
    }

    public async Task MarkEscalatedAsync(Guid conversationId, Guid ticketId)
    {
        var conversation = await _dbContext.Conversations.FindAsync(conversationId);
        if (conversation is null) return;

        conversation.IsEscalated = true;
        conversation.LinkedTicketId = ticketId;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<PagedResult<ConversationSummary>> GetPagedSummariesAsync(int page, int pageSize)
    {
        int totalCount = await _dbContext.Conversations.CountAsync();

        var rawItems = await _dbContext.Conversations
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id,
                c.CreatedAt,
                c.IsEscalated,
                c.LinkedTicketId,
                MessageCount = c.Messages.Count,
                LastMessage = c.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => m.Content)
                    .FirstOrDefault(),
                AverageSentimentScore = c.Messages
                    .Where(m => m.SentimentScore != null)
                    .Average(m => (double?)m.SentimentScore)
            })
            .ToListAsync();

        // Truncation happens after materializing — only PageSize rows are in
        // memory at this point, so this is not a scale concern.
        var items = rawItems
            .Select(r => new ConversationSummary(
                r.Id,
                r.CreatedAt,
                r.IsEscalated,
                r.LinkedTicketId,
                r.MessageCount,
                Truncate(r.LastMessage),
                r.AverageSentimentScore))
            .ToList();

        return new PagedResult<ConversationSummary>(items, totalCount, page, pageSize);
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _dbContext.Conversations.CountAsync();
    }

    public async Task<int> GetEscalatedCountAsync()
    {
        return await _dbContext.Conversations.CountAsync(c => c.IsEscalated);
    }

    public async Task<double?> GetAverageSentimentScoreAsync()
    {
        bool anyScored = await _dbContext.Messages.AnyAsync(m => m.SentimentScore != null);
        if (!anyScored) return null;

        return await _dbContext.Messages
            .Where(m => m.SentimentScore != null)
            .AverageAsync(m => m.SentimentScore);
    }

    private static string? Truncate(string? text)
    {
        if (text is null) return null;
        return text.Length <= PreviewMaxLength ? text : text[..PreviewMaxLength] + "...";
    }
}