using SupportPilot.Application.Interfaces;

namespace SupportPilot.Infrastructure.Configuration;

/// <summary>
/// Retrieval-augmented generation tuning. RelevanceThreshold guards against
/// hallucination: if no retrieved chunk scores above it, the assistant says it
/// doesn't know instead of answering from the model's own knowledge.
/// Implements IRagOptions so the Application layer can consume it without
/// depending on Infrastructure.
/// </summary>
public class RagSettings : IRagOptions
{
    /// <summary>Minimum cosine score for a retrieved chunk to be used as context.</summary>
    public float RelevanceThreshold { get; set; } = 0.3f;

    /// <summary>How many chunks to retrieve from the vector store per question.</summary>
    public int TopK { get; set; } = 3;

    /// <summary>
    /// System prompt for grounded answering. "{0}" is replaced with the
    /// retrieved context block at runtime.
    /// </summary>
    public string SystemPromptTemplate { get; set; } =
        "You are a support assistant. Answer the user's question using ONLY the " +
        "context provided below. If the context does not contain the answer, say " +
        "you don't have that information. Do not use outside knowledge.\n\n" +
        "Context:\n{0}";
}