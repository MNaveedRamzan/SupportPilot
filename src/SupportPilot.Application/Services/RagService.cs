using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using SupportPilot.Application.DTOs;
using SupportPilot.Application.Interfaces;
using SupportPilot.Domain.Common;
using SupportPilot.Domain.Enums;

namespace SupportPilot.Application.Services;

public class RagService : IRagService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;
    private readonly IChatProvider _chatProvider;
    private readonly IRagOptions _options;
    private readonly ILogger<RagService> _logger;

    public RagService(
        IEmbeddingService embeddingService,
        IVectorStore vectorStore,
        IChatProvider chatProvider,
        IRagOptions options,
        ILogger<RagService> logger)
    {
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _chatProvider = chatProvider;
        _options = options;
        _logger = logger;
    }

    public async Task<ChatAnswer> AskAsync(ChatRequest request)
    {
        var (relevant, topScore) = await RetrieveAsync(request.Message);

        if (relevant.Count == 0)
        {
            return new ChatAnswer(
                Content: "I don't have information about that in the knowledge base.",
                AnsweredFromKnowledgeBase: false,
                RetrievedChunks: 0,
                TopScore: topScore,
                InputTokens: 0,
                OutputTokens: 0);
        }

        var conversation = BuildGroundedConversation(request.Message, relevant);
        ChatResponse response = await _chatProvider.SendMessageAsync(conversation);

        _logger.LogInformation(
            "RAG answer generated. Chunks: {Chunks}, TopScore: {TopScore}, " +
            "InputTokens: {Input}, OutputTokens: {Output}",
            relevant.Count, relevant[0].Score, response.InputTokens, response.OutputTokens);

        return new ChatAnswer(
            Content: response.Content,
            AnsweredFromKnowledgeBase: true,
            RetrievedChunks: relevant.Count,
            TopScore: relevant[0].Score,
            InputTokens: response.InputTokens,
            OutputTokens: response.OutputTokens);
    }

    public async IAsyncEnumerable<string> AskStreamingAsync(
        ChatRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var (relevant, _) = await RetrieveAsync(request.Message);

        if (relevant.Count == 0)
        {
            yield return "I don't have information about that in the knowledge base.";
            yield break;
        }

        var conversation = BuildGroundedConversation(request.Message, relevant);

        await foreach (string chunk in _chatProvider.StreamMessageAsync(conversation, cancellationToken))
        {
            yield return chunk;
        }

        _logger.LogInformation(
            "RAG streaming answer completed. Chunks: {Chunks}, TopScore: {TopScore}",
            relevant.Count, relevant[0].Score);
    }

    /// <summary>
    /// Embeds the question, retrieves candidate chunks, and filters out weak
    /// matches. Shared by both the blocking and streaming ask paths.
    /// </summary>
    private async Task<(List<SearchResult> Relevant, float TopScore)> RetrieveAsync(string question)
    {
        float[] queryVector = await _embeddingService.GetEmbeddingAsync(question);
        IReadOnlyList<SearchResult> results = await _vectorStore.SearchAsync(queryVector, _options.TopK);

        var relevant = results.Where(r => r.Score >= _options.RelevanceThreshold).ToList();
        float topScore = results.Count > 0 ? results[0].Score : 0f;

        if (relevant.Count == 0)
        {
            _logger.LogInformation(
                "No relevant context found. TopScore: {TopScore}, Threshold: {Threshold}",
                topScore, _options.RelevanceThreshold);
        }

        return (relevant, topScore);
    }

    /// <summary>
    /// Builds the grounded conversation (system prompt + retrieved context + question).
    /// Shared by both the blocking and streaming ask paths.
    /// </summary>
    private List<ChatTurn> BuildGroundedConversation(string question, List<SearchResult> relevant)
    {
        string context = string.Join("\n", relevant.Select(r => $"- {r.Text}"));
        string systemPrompt = string.Format(_options.SystemPromptTemplate, context);

        return new List<ChatTurn>
        {
            new ChatTurn(ChatRole.System, systemPrompt),
            new ChatTurn(ChatRole.User, question)
        };
    }
}