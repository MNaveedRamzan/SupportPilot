namespace SupportPilot.Application.Interfaces;

/// <summary>
/// Converts text into embedding vectors for semantic search / RAG retrieval.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Converts a single piece of text into an embedding vector.
    /// </summary>
    Task<float[]> GetEmbeddingAsync(string text);
}