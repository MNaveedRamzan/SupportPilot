using Microsoft.Extensions.Logging;
using SupportPilot.Application.Interfaces;
using SupportPilot.Domain.Common;
using SupportPilot.Infrastructure.Configuration;

namespace SupportPilot.Infrastructure.AI;

/// <summary>
/// Decorator that adds retry-with-exponential-backoff to any IChatProvider.
/// Implements IChatProvider and wraps another IChatProvider, so callers cannot
/// tell the difference — retry behavior is added without modifying provider code
/// (Open/Closed Principle). Retry logic lives here once instead of being
/// duplicated in every provider implementation.
/// </summary>
public class ResilientChatProvider : IChatProvider
{
    private readonly IChatProvider _inner;
    private readonly RetrySettings _settings;
    private readonly ILogger<ResilientChatProvider> _logger;

    public string Name => _inner.Name;
    public string ModelName => _inner.ModelName;

    public ResilientChatProvider(
        IChatProvider inner,
        RetrySettings settings,
        ILogger<ResilientChatProvider> logger)
    {
        _inner = inner;
        _settings = settings;
        _logger = logger;
    }

    public async Task<ChatResponse> SendMessageAsync(List<ChatTurn> conversation)
    {
        int delayMs = _settings.InitialDelayMs;

        for (int attempt = 1; attempt <= _settings.MaxAttempts; attempt++)
        {
            try
            {
                return await _inner.SendMessageAsync(conversation);
            }
            catch (Exception ex) when (IsTransient(ex) && attempt < _settings.MaxAttempts)
            {
                _logger.LogWarning(ex,
                    "Transient failure calling {Provider} (attempt {Attempt}/{MaxAttempts}). " +
                    "Retrying in {DelayMs}ms.",
                    _inner.Name, attempt, _settings.MaxAttempts, delayMs);

                await Task.Delay(delayMs);
                delayMs *= _settings.BackoffMultiplier;
            }
        }

        _logger.LogWarning(
            "Final retry attempt for {Provider} (attempt {Attempt}/{MaxAttempts}).",
            _inner.Name, _settings.MaxAttempts, _settings.MaxAttempts);

        return await _inner.SendMessageAsync(conversation);
    }

    /// <summary>
    /// Streaming bypasses retry entirely: once tokens have been yielded to the
    /// caller, a retry would re-send already-delivered text. Retry semantics
    /// only make sense before any output has been committed, so this method
    /// passes straight through to the inner provider by design.
    /// </summary>
    public IAsyncEnumerable<string> StreamMessageAsync(
        List<ChatTurn> conversation,
        CancellationToken cancellationToken = default)
    {
        return _inner.StreamMessageAsync(conversation, cancellationToken);
    }

    private static bool IsTransient(Exception ex) => ex switch
    {
        HttpRequestException => true,
        TaskCanceledException => true,
        TimeoutException => true,
        _ => false
    };
}