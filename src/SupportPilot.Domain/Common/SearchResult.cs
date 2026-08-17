// src/Domain/Common/SearchResult.cs
namespace SupportPilot.Domain.Common;

/// <summary>
/// A single semantic search hit — the stored text and its similarity score.
/// </summary>
public record SearchResult(string Text, float Score);