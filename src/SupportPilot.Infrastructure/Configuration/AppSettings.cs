namespace SupportPilot.Infrastructure.Configuration;

/// <summary>
/// Root configuration object bound from appsettings.json and environment variables.
/// </summary>
public class AppSettings
{
    public AIProviderSettings AIProvider { get; set; } = new();
    public OpenAISettings OpenAI { get; set; } = new();
    public AnthropicSettings Anthropic { get; set; } = new();
    public RetrySettings Retry { get; set; } = new();
    public QdrantSettings Qdrant { get; set; } = new();
    public EmbeddingSettings Embedding { get; set; } = new();
    public RagSettings Rag { get; set; } = new();
    public SentimentSettings Sentiment { get; set; } = new();
}