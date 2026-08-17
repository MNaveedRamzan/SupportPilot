namespace SupportPilot.Infrastructure.Configuration;

/// <summary>
/// Qdrant vector database connection and collection settings.
/// ApiKey is bound from configuration (env var "Qdrant__ApiKey" or user secrets),
/// never read directly from the environment inside the service.
/// </summary>
public class QdrantSettings
{
    // Cluster host WITHOUT "https://" prefix and WITHOUT port
    // e.g. "e3c97445-....us-east-2-0.aws.cloud.qdrant.io"
    public string Host { get; set; } = "";

    public int Port { get; set; } = 6334; // gRPC port (not 6333, that's REST)

    public string CollectionName { get; set; } = "kb_articles";

    public int VectorSize { get; set; } = 1536; // must match text-embedding-3-small

    public string ApiKey { get; set; } = "";
}