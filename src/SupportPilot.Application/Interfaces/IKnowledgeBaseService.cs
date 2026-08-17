using SupportPilot.Domain.Common;

namespace SupportPilot.Application.Interfaces;

/// <summary>
/// Orchestrates knowledge base article management for the admin dashboard.
/// Sits above IVectorStore because adding an article requires embedding the
/// text first — a concern the vector store itself should not own.
/// </summary>
public interface IKnowledgeBaseService
{
    Task<KnowledgeArticle> AddArticleAsync(string text);
    Task<IReadOnlyList<KnowledgeArticle>> GetAllAsync(int limit = 100);
    Task DeleteArticleAsync(string id);
}