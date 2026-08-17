namespace SupportPilot.Application.DTOs;

/// <summary>
/// Request payload for adding a new knowledge base article.
/// </summary>
public record AddArticleRequest(string Text);