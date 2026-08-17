namespace SupportPilot.Domain.Common;

/// <summary>
/// A knowledge base article as stored in the vector store, with its point ID
/// so it can be individually deleted. Distinct from SearchResult, which is
/// search-specific (has a relevance Score, not an Id).
/// </summary>
public record KnowledgeArticle(string Id, string Text);