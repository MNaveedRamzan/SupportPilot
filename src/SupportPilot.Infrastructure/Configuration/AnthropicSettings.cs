namespace SupportPilot.Infrastructure.Configuration;

/// <summary>
/// Anthropic chat model configuration.
/// ApiKey is bound from configuration (user secrets in dev, env var "Anthropic__ApiKey"
/// in production), never read directly from the environment inside the provider.
/// </summary>
public class AnthropicSettings
{
    public string Model { get; set; } = "claude-haiku-4-5-20251001";
    public string SystemPrompt { get; set; } = "You are a helpful assistant.";
    public int MaxTokens { get; set; } = 1024;
    public string ApiKey { get; set; } = "";
}