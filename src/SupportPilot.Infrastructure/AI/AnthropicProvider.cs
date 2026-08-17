using System.Runtime.CompilerServices;
using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Logging;
using SupportPilot.Application.Interfaces;
using SupportPilot.Domain.Common;
using SupportPilot.Infrastructure.Configuration;
using ChatRole = SupportPilot.Domain.Enums.ChatRole;

namespace SupportPilot.Infrastructure.AI;

public class AnthropicProvider : IChatProvider
{
    private readonly AnthropicClient _client;
    private readonly AnthropicSettings _settings;
    private readonly ILogger<AnthropicProvider> _logger;

    public string Name => "Anthropic";
    public string ModelName => _settings.Model;

    public AnthropicProvider(AnthropicSettings settings, ILogger<AnthropicProvider> logger)
    {
        _settings = settings;
        _logger = logger;
        _client = new AnthropicClient { ApiKey = settings.ApiKey };
    }

    public async Task<ChatResponse> SendMessageAsync(List<ChatTurn> conversation)
    {
        var parameters = BuildParams(conversation);

        _logger.LogDebug(
            "Anthropic request: Model: {Model}, MaxTokens: {MaxTokens}, MessageCount: {Count}",
            _settings.Model, _settings.MaxTokens, parameters.Messages.Count);

        var response = await _client.Messages.Create(parameters);

        string responseText = "";
        foreach (var block in response.Content)
        {
            if (block.TryPickText(out var textBlock))
            {
                responseText += textBlock.Text;
            }
        }

        return new ChatResponse(
            Content: responseText,
            InputTokens: (int)response.Usage.InputTokens,
            OutputTokens: (int)response.Usage.OutputTokens);
    }

    public async IAsyncEnumerable<string> StreamMessageAsync(
        List<ChatTurn> conversation,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var parameters = BuildParams(conversation);

        _logger.LogDebug(
            "Anthropic streaming request: Model: {Model}, MessageCount: {Count}",
            _settings.Model, parameters.Messages.Count);

        await foreach (var streamEvent in _client.Messages.CreateStreaming(parameters, cancellationToken))
        {
            if (streamEvent.TryPickContentBlockDelta(out var delta) &&
                delta.Delta.TryPickText(out var textDelta))
            {
                yield return textDelta.Text;
            }
        }
    }

    private MessageCreateParams BuildParams(List<ChatTurn> conversation)
    {
        string systemPrompt = conversation
            .FirstOrDefault(t => t.Role == ChatRole.System)?.Content
            ?? "You are a helpful assistant.";

        var messagesList = new List<MessageParam>();
        foreach (var turn in conversation.Where(t => t.Role != ChatRole.System))
        {
            messagesList.Add(new MessageParam
            {
                Role = turn.Role == ChatRole.User ? Role.User : Role.Assistant,
                Content = turn.Content
            });
        }

        return new MessageCreateParams
        {
            Model = _settings.Model,
            MaxTokens = _settings.MaxTokens,
            Messages = messagesList,
            System = systemPrompt
        };
    }
}