using Microsoft.Extensions.Logging;
using OpenAI.Embeddings;
using SupportPilot.Application.Interfaces;
using SupportPilot.Infrastructure.Configuration;

namespace SupportPilot.Infrastructure.Embeddings;

/// <summary>
/// OpenAI implementation of IEmbeddingService. Converts text into fixed-length
/// vectors used for semantic search and RAG retrieval. The embedding model is
/// separate from the chat model and is configured independently.
/// </summary>
public class EmbeddingService : IEmbeddingService
{
    private readonly EmbeddingClient _client;
    private readonly EmbeddingSettings _settings;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(
        EmbeddingSettings settings,
        OpenAISettings openAiSettings,
        ILogger<EmbeddingService> logger)
    {
        _settings = settings;
        _logger = logger;
        _client = new EmbeddingClient(settings.Model, openAiSettings.ApiKey);
    }

    /// <summary>
    /// Converts a single piece of text into an embedding vector.
    /// </summary>
    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        OpenAIEmbedding embedding = await _client.GenerateEmbeddingAsync(text);
        float[] vector = embedding.ToFloats().ToArray();

        _logger.LogDebug(
            "Embedding generated. TextLength: {TextLength}, Dimensions: {Dimensions}",
            text.Length, vector.Length);

        return vector;
    }
}