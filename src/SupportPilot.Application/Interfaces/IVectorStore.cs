using SupportPilot.Domain.Common;

namespace SupportPilot.Application.Interfaces;

public interface IVectorStore
{
    Task EnsureCollectionExistsAsync();

    /// <summary>
    /// Stores a piece of text together with its embedding vector.
    /// Returns the generated point id so callers (like KnowledgeBaseService)
    /// can reference the newly created article without a follow-up query.
    /// </summary>
    Task<string> UpsertTextAsync(string text, float[] vector);

    Task<IReadOnlyList<SearchResult>> SearchAsync(float[] queryVector, int limit = 3);
    Task<IReadOnlyList<KnowledgeArticle>> ListAllAsync(int limit = 100);
    Task DeleteAsync(string id);
}