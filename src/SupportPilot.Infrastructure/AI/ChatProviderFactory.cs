using Microsoft.Extensions.Logging;
using SupportPilot.Application.Interfaces;
using SupportPilot.Infrastructure.Configuration;

namespace SupportPilot.Infrastructure.AI;

/// <summary>
/// Creates the active IChatProvider based on configuration, then wraps it in
/// the resilience decorator. Factory decides WHICH provider to build;
/// the decorator adds retry behavior to whatever was built — so retry logic
/// is written once and applies to every provider automatically.
/// </summary>
public class ChatProviderFactory
{
    private readonly AppSettings _settings;
    private readonly ILoggerFactory _loggerFactory;

    public ChatProviderFactory(AppSettings settings, ILoggerFactory loggerFactory)
    {
        _settings = settings;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Builds the configured provider wrapped in retry handling.
    /// </summary>
    public IChatProvider Create()
    {
        IChatProvider provider = _settings.AIProvider.Active.ToLowerInvariant() switch
        {
            "openai" => new OpenAIProvider(
                _settings.OpenAI,
                _loggerFactory.CreateLogger<OpenAIProvider>()),

            "anthropic" => new AnthropicProvider(
                _settings.Anthropic,
                _loggerFactory.CreateLogger<AnthropicProvider>()),

            _ => throw new InvalidOperationException(
                $"Unknown provider: {_settings.AIProvider.Active}. Use 'openai' or 'anthropic'.")
        };

        // Wrap the concrete provider with retry behavior.
        return new ResilientChatProvider(
            provider,
            _settings.Retry,
            _loggerFactory.CreateLogger<ResilientChatProvider>());
    }
}