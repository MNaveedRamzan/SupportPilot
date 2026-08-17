namespace SupportPilot.Infrastructure.Configuration;

/// <summary>
/// Embedding model configuration. Dimensions must match QdrantSettings.VectorSize —
/// the vector database collection is created with a fixed vector size, so changing
/// the embedding model requires a matching collection.
/// Uses the OpenAI API key from OpenAISettings.
/// </summary>
public class EmbeddingSettings
{
    public string Model { get; set; } = "text-embedding-3-small";
    public int Dimensions { get; set; } = 1536;
}