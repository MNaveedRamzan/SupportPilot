namespace SupportPilot.Infrastructure.Configuration;

/// <summary>
/// Selects which chat provider is active at runtime ("openai" or "anthropic").
/// </summary>
public class AIProviderSettings
{
    public string Active { get; set; } = "openai";
}