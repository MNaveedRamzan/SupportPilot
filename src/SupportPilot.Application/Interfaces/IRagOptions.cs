namespace SupportPilot.Application.Interfaces;

/// <summary>
/// Read-only RAG tuning values needed by the Application layer.
/// Defined here (not in Infrastructure) so Application depends on an
/// abstraction it owns, keeping the dependency direction pointing inward.
/// </summary>
public interface IRagOptions
{
    /// <summary>Minimum cosine score for a retrieved chunk to be used as context.</summary>
    float RelevanceThreshold { get; }

    /// <summary>How many chunks to retrieve from the vector store per question.</summary>
    int TopK { get; }

    /// <summary>System prompt for grounded answering; "{0}" is replaced with the context.</summary>
    string SystemPromptTemplate { get; }
}