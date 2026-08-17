namespace SupportPilot.Infrastructure.Configuration;

/// <summary>
/// OpenAI chat model configuration.
/// ApiKey is bound from configuration (user secrets in dev, env var "OpenAI__ApiKey"
/// in production), never read directly from the environment inside the provider.
/// </summary>
public class OpenAISettings
{
    public string Model { get; set; } = "gpt-4o-mini";
    public string SystemPrompt { get; set; } = "You are a helpful assistant.";
    public string ApiKey { get; set; } = "";
}