using Microsoft.Extensions.Logging;
using SupportPilot.Application.Interfaces;
using SupportPilot.Domain.Common;
using SupportPilot.Domain.Enums;
using System.Text.Json;

namespace SupportPilot.Infrastructure.Sentiment;

/// <summary>
/// LLM-based sentiment scorer. Reuses the existing IChatProvider abstraction
/// so it works with whichever provider is configured and benefits from the
/// same retry/decorator behavior as the rest of the app.
/// </summary>
public class SentimentAnalyzer : ISentimentAnalyzer
{
    private const string SystemPrompt =
        "You are a sentiment classifier for customer support messages. " +
        "Given a customer's message, respond with ONLY a JSON object in this " +
        "exact shape: {\"score\": 0.0} where score is a number from 0.0 (calm, " +
        "neutral) to 1.0 (extremely frustrated or angry). No explanation, no " +
        "other text, no markdown.";

    private readonly IChatProvider _chatProvider;
    private readonly ILogger<SentimentAnalyzer> _logger;

    public SentimentAnalyzer(IChatProvider chatProvider, ILogger<SentimentAnalyzer> logger)
    {
        _chatProvider = chatProvider;
        _logger = logger;
    }

    public async Task<float> AnalyzeAsync(string message)
    {
        var conversation = new List<ChatTurn>
        {
            new ChatTurn(ChatRole.System, SystemPrompt),
            new ChatTurn(ChatRole.User, message)
        };

        try
        {
            ChatResponse response = await _chatProvider.SendMessageAsync(conversation);
            using var doc = JsonDocument.Parse(response.Content);
            float score = doc.RootElement.GetProperty("score").GetSingle();
            return Math.Clamp(score, 0f, 1f);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Sentiment analysis failed, defaulting to neutral (0.0).");
            return 0f;
        }
    }
}