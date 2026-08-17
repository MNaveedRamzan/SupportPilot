using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using SupportPilot.Application.Interfaces;
using SupportPilot.Domain.Common;
using SupportPilot.Domain.Enums;
using SupportPilot.Infrastructure.Configuration;

namespace SupportPilot.Infrastructure.AI;

/// <summary>
/// OpenAI implementation of IChatProvider. Adapts the provider-agnostic
/// ChatTurn model to OpenAI's SDK-specific ChatMessage types.
/// Contains no retry logic — that is added by a decorator (ResilientChatProvider),
/// keeping this class focused on a single responsibility: talking to OpenAI.
/// </summary>
public class OpenAIProvider : IChatProvider
{
    private readonly ChatClient _client;
    private readonly OpenAISettings _settings;
    private readonly ILogger<OpenAIProvider> _logger;

    public string Name => "OpenAI";
    public string ModelName => _settings.Model;

    public OpenAIProvider(OpenAISettings settings, ILogger<OpenAIProvider> logger)
    {
        _settings = settings;
        _logger = logger;
        _client = new ChatClient(model: settings.Model, apiKey: settings.ApiKey);
    }

    public async Task<ChatResponse> SendMessageAsync(List<ChatTurn> conversation)
    {
        var messages = ToOpenAIMessages(conversation);

        _logger.LogDebug("OpenAI request: {MessageCount} messages, Model: {Model}",
            messages.Count, _settings.Model);

        ChatCompletion completion = await _client.CompleteChatAsync(messages);

        return new ChatResponse(
            Content: completion.Content[0].Text,
            InputTokens: completion.Usage.InputTokenCount,
            OutputTokens: completion.Usage.OutputTokenCount);
    }

    public async IAsyncEnumerable<string> StreamMessageAsync(
        List<ChatTurn> conversation,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messages = ToOpenAIMessages(conversation);

        _logger.LogDebug("OpenAI streaming request: {MessageCount} messages, Model: {Model}",
            messages.Count, _settings.Model);

        await foreach (StreamingChatCompletionUpdate update in
            _client.CompleteChatStreamingAsync(messages, cancellationToken: cancellationToken))
        {
            foreach (ChatMessageContentPart part in update.ContentUpdate)
            {
                if (!string.IsNullOrEmpty(part.Text))
                {
                    yield return part.Text;
                }
            }
        }
    }

    /// <summary>
    /// Converts generic ChatTurn history into OpenAI-specific ChatMessage types.
    /// Shared by both the blocking and streaming send paths.
    /// </summary>
    private static List<ChatMessage> ToOpenAIMessages(List<ChatTurn> conversation)
    {
        return conversation.Select(turn => (ChatMessage)(turn.Role switch
        {
            ChatRole.System => new SystemChatMessage(turn.Content),
            ChatRole.User => new UserChatMessage(turn.Content),
            ChatRole.Assistant => new AssistantChatMessage(turn.Content),
            _ => throw new ArgumentException($"Unknown role: {turn.Role}")
        })).ToList();
    }
}