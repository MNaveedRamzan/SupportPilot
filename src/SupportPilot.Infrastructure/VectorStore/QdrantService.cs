using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using SupportPilot.Application.Interfaces;
using SupportPilot.Domain.Common;
using SupportPilot.Infrastructure.Configuration;

namespace SupportPilot.Infrastructure.VectorStore;

/// <summary>
/// Qdrant implementation of IVectorStore. Handles collection setup, storing
/// embedded text as points, and semantic search via cosine similarity.
/// Callers depend on IVectorStore, so swapping Qdrant for another vector
/// database would not require changes outside this class.
/// </summary>
public class QdrantService : IVectorStore
{
    private readonly QdrantClient _client;
    private readonly QdrantSettings _settings;
    private readonly ILogger<QdrantService> _logger;

    public QdrantService(QdrantSettings settings, ILogger<QdrantService> logger)
    {
        _settings = settings;
        _logger = logger;

        // gRPC client — host WITHOUT scheme, port 6334, https: true for cloud
        _client = new QdrantClient(
            host: settings.Host,
            port: settings.Port,
            https: true,
            apiKey: settings.ApiKey);
    }

    /// <summary>
    /// Creates the collection if it doesn't already exist.
    /// Vector size and distance metric are fixed at creation time —
    /// they must match the embedding model's output (1536 dims, Cosine).
    /// </summary>
    public async Task EnsureCollectionExistsAsync()
    {
        bool exists = await _client.CollectionExistsAsync(_settings.CollectionName);

        if (!exists)
        {
            await _client.CreateCollectionAsync(
                collectionName: _settings.CollectionName,
                vectorsConfig: new VectorParams
                {
                    Size = (ulong)_settings.VectorSize,
                    Distance = Distance.Cosine
                });

            _logger.LogInformation("Qdrant collection created: {Collection}",
                _settings.CollectionName);
        }
        else
        {
            _logger.LogInformation("Qdrant collection already exists: {Collection}",
                _settings.CollectionName);
        }
    }

    /// <summary>
    /// Stores a piece of text with its embedding vector. The original text
    /// is kept in the payload so search results are human-readable.
    /// </summary>
    public async Task<string> UpsertTextAsync(string text, float[] vector)
    {
        string id = Guid.NewGuid().ToString();

        var point = new PointStruct
        {
            Id = new PointId { Uuid = id },
            Vectors = vector,
            Payload = { ["text"] = text }
        };

        await _client.UpsertAsync(_settings.CollectionName, new List<PointStruct> { point });

        _logger.LogInformation(
            "Point upserted. Collection: {Collection}, Id: {Id}, TextLength: {Length}",
            _settings.CollectionName, id, text.Length);

        return id;
    }

    /// <summary>
    /// Searches for the top-K most semantically similar stored texts,
    /// using cosine similarity computed server-side by Qdrant.
    /// </summary>
    public async Task<IReadOnlyList<SearchResult>> SearchAsync(float[] queryVector, int limit = 3)
    {
        var results = await _client.SearchAsync(
            _settings.CollectionName, queryVector, limit: (ulong)limit);

        return results
            .Select(r => new SearchResult(r.Payload["text"].StringValue, r.Score))
            .ToList();
    }

    /// <summary>
    /// Lists stored articles without their vectors, for admin display.
    /// Uses Qdrant's scroll API, which pages through all points in a
    /// collection (unlike Search, which needs a query vector).
    /// </summary>
    public async Task<IReadOnlyList<KnowledgeArticle>> ListAllAsync(int limit = 100)
    {
        var response = await _client.ScrollAsync(
            _settings.CollectionName,
            limit: (uint)limit,
            payloadSelector: true,
            vectorsSelector: false);

        return response.Result
            .Select(p => new KnowledgeArticle(p.Id.Uuid, p.Payload["text"].StringValue))
            .ToList();
    }

    /// <summary>
    /// Deletes a single article by its point id.
    /// </summary>
    public async Task DeleteAsync(string id)
    {
        await _client.DeleteAsync(_settings.CollectionName, new List<Guid> { Guid.Parse(id) });

        _logger.LogInformation("Point deleted. Collection: {Collection}, Id: {Id}",
            _settings.CollectionName, id);
    }
}