using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SupportPilot.Application.DTOs;
using SupportPilot.Application.Interfaces;
using SupportPilot.Domain.Common;
using SupportPilot.Domain.Entities;
using SupportPilot.Domain.Enums;
using SupportPilot.Infrastructure.Configuration;

namespace SupportPilot.Api.Hubs;

/// <summary>
/// Real-time chat hub. Streams RAG answers token by token, persists every
/// turn to the conversation, and auto-escalates to a support ticket when a
/// user message's sentiment score crosses the configured threshold.
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IRagService _ragService;
    private readonly IConversationRepository _conversationRepository;
    private readonly ISentimentAnalyzer _sentimentAnalyzer;
    private readonly ITicketRepository _ticketRepository;
    private readonly SentimentSettings _sentimentSettings;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
        IRagService ragService,
        IConversationRepository conversationRepository,
        ISentimentAnalyzer sentimentAnalyzer,
        ITicketRepository ticketRepository,
        SentimentSettings sentimentSettings,
        ILogger<ChatHub> logger)
    {
        _ragService = ragService;
        _conversationRepository = conversationRepository;
        _sentimentAnalyzer = sentimentAnalyzer;
        _ticketRepository = ticketRepository;
        _sentimentSettings = sentimentSettings;
        _logger = logger;
    }

    /// <summary>
    /// Invoked by the client to ask a question. If conversationId is null or
    /// unknown, a new conversation is created and its id sent back via
    /// "ConversationStarted" before streaming begins.
    /// </summary>
    public async Task Ask(string message, string? conversationId)
    {
        _logger.LogInformation("SignalR chat request received. ConnectionId: {ConnectionId}",
            Context.ConnectionId);

        var conversation = await ResolveConversationAsync(conversationId);

        // Persist the user's turn first, so it's saved even if sentiment
        // analysis or the RAG call fails downstream.
        Message userMessage = await _conversationRepository.AddMessageAsync(
            conversation.Id, ChatRole.User, message);

        float sentimentScore = await _sentimentAnalyzer.AnalyzeAsync(message);
        await _conversationRepository.UpdateMessageSentimentAsync(userMessage.Id, sentimentScore);

        if (EscalationPolicy.ShouldEscalate(
            conversation.IsEscalated, sentimentScore, _sentimentSettings.EscalationThreshold))
        {
            await EscalateAsync(conversation.Id, message, sentimentScore);
        }

        var request = new ChatRequest(message);
        string fullResponse = string.Empty;

        await foreach (string chunk in _ragService.AskStreamingAsync(
            request, Context.ConnectionAborted))
        {
            fullResponse += chunk;
            await Clients.Caller.SendAsync("ReceiveChunk", chunk);
        }

        await _conversationRepository.AddMessageAsync(conversation.Id, ChatRole.Assistant, fullResponse);

        await Clients.Caller.SendAsync("ReceiveComplete");
    }

    private async Task<Domain.Entities.Conversation> ResolveConversationAsync(string? conversationId)
    {
        if (Guid.TryParse(conversationId, out Guid parsedId))
        {
            var existing = await _conversationRepository.GetByIdAsync(parsedId);
            if (existing is not null)
            {
                return existing;
            }
        }

        var created = await _conversationRepository.CreateAsync();
        await Clients.Caller.SendAsync("ConversationStarted", created.Id);
        return created;
    }

    private async Task UpdateLastUserMessageSentimentAsync(Guid conversationId, float score)
    {
        // AddMessageAsync already saved the message without a score (analysis
        // runs after persistence to guarantee the message is never lost even
        // if the analyzer fails). This call attaches the score right after.
        var conversation = await _conversationRepository.GetByIdAsync(conversationId);
        var lastUserMessage = conversation?.Messages
            .Where(m => m.Role == ChatRole.User)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefault();

        if (lastUserMessage is not null)
        {
            lastUserMessage.SentimentScore = score;
        }
    }

    private async Task EscalateAsync(Guid conversationId, string message, float score)
    {
        var ticket = await _ticketRepository.CreateAsync(
            subject: "Auto-escalated: customer frustration detected",
            description: $"Sentiment score {score:F2} exceeded threshold on message: \"{message}\"");

        await _conversationRepository.MarkEscalatedAsync(conversationId, ticket.Id);

        _logger.LogInformation(
            "Conversation {ConversationId} auto-escalated to ticket {TicketId}. Score: {Score}",
            conversationId, ticket.Id, score);

        await Clients.Caller.SendAsync("Escalated", ticket.Id);
    }
}