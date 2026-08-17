using SupportPilot.Application.Interfaces;
using SupportPilot.Domain.Common;

namespace SupportPilot.Application.Services;

public class KnowledgeBaseService : IKnowledgeBaseService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;

    public KnowledgeBaseService(IEmbeddingService embeddingService, IVectorStore vectorStore)
    {
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
    }

    public async Task<KnowledgeArticle> AddArticleAsync(string text)
    {
        float[] vector = await _embeddingService.GetEmbeddingAsync(text);
        string id = await _vectorStore.UpsertTextAsync(text, vector);
        return new KnowledgeArticle(id, text);
    }

    public async Task<IReadOnlyList<KnowledgeArticle>> GetAllAsync(int limit = 100)
    {
        return await _vectorStore.ListAllAsync(limit);
    }

    public async Task DeleteArticleAsync(string id)
    {
        await _vectorStore.DeleteAsync(id);
    }
}