using SupportPilot.Domain.Common;

namespace SupportPilot.Application.Interfaces;

/// <summary>
/// Common interface for AI chat providers (OpenAI, Anthropic, etc.).
/// Enables provider-agnostic chat logic in the application layer.
/// </summary>
public interface IChatProvider
{
    /// <summary>
    /// Provider identifier for logging and display purposes.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Model identifier being used by this provider.
    /// </summary>
    string ModelName { get; }

    /// <summary>
    /// Sends the conversation history and returns the assistant's reply.
    /// </summary>
    /// <param name="conversation">Full conversation history including system prompt.</param>
    /// <returns>Response containing assistant text and token usage.</returns>
    Task<ChatResponse> SendMessageAsync(List<ChatTurn> conversation);

    /// <summary>
    /// Streams the assistant's reply token by token as it is generated.
    /// Unlike SendMessageAsync, this does not report token usage — once streaming
    /// begins the response is committed and cannot be retried, so it bypasses the
    /// resilience decorator by design.
    /// </summary>
    /// <param name="conversation">Full conversation history including system prompt.</param>
    /// <returns>An async stream of text chunks in generation order.</returns>

    IAsyncEnumerable<string> StreamMessageAsync(List<ChatTurn> conversation, CancellationToken cancellationToken = default);
}