using SupportPilot.Application.DTOs;

namespace SupportPilot.Application.Interfaces;

/// <summary>
/// Answers questions grounded in the knowledge base: retrieves relevant context
/// from the vector store, then asks the chat provider to answer using only that context.
/// </summary>
public interface IRagService
{
    Task<ChatAnswer> AskAsync(ChatRequest request);

    /// <summary>
    /// Same retrieval and grounding logic as AskAsync, but streams the answer
    /// token by token. If no relevant context is found, yields a single
    /// "don't know" message instead of calling the chat provider.
    /// </summary>
    IAsyncEnumerable<string> AskStreamingAsync(ChatRequest request, CancellationToken cancellationToken = default);
}