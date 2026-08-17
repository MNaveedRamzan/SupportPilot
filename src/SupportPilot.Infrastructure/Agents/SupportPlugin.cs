using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using SupportPilot.Application.Interfaces;

namespace SupportPilot.Infrastructure.Agents;

/// <summary>
/// Tools exposed to the Semantic Kernel agent. The LLM decides on its own,
/// based on the conversation, whether and when to call each function —
/// this class only defines what each tool does, not when it's used.
/// </summary>
public class SupportPlugin
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRagOptions _ragOptions;

    public SupportPlugin(
        IEmbeddingService embeddingService,
        IVectorStore vectorStore,
        IServiceScopeFactory scopeFactory,
        IRagOptions ragOptions)
    {
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _scopeFactory = scopeFactory;
        _ragOptions = ragOptions;
    }

    [KernelFunction("search_kb")]
    [Description("Searches the knowledge base for articles relevant to a customer's question. " +
                 "Use this before answering any question that might be covered by company policy, " +
                 "such as refunds, billing, password resets, or subscriptions.")]
    public async Task<string> SearchKnowledgeBaseAsync(
        [Description("The customer's question or search phrase")] string query)
    {
        float[] queryVector = await _embeddingService.GetEmbeddingAsync(query);
        var results = await _vectorStore.SearchAsync(queryVector, _ragOptions.TopK);

        var relevant = results.Where(r => r.Score >= _ragOptions.RelevanceThreshold).ToList();

        if (relevant.Count == 0)
        {
            return "No relevant articles found in the knowledge base.";
        }

        return string.Join("\n", relevant.Select(r => $"- {r.Text}"));
    }

    [KernelFunction("create_ticket")]
    [Description("Creates a support ticket for issues the knowledge base cannot resolve, " +
                 "or when the customer explicitly asks to speak with a human agent. " +
                 "Only use this after search_kb has failed to find an answer.")]
    public async Task<string> CreateTicketAsync(
        [Description("A short summary of the customer's issue")] string subject,
        [Description("Full details of the customer's issue, including what was already tried")] string description)
    {
        // Singleton plugin can't hold a Scoped repository directly (captive dependency).
        // Instead we open a short-lived scope per tool-call, resolve a fresh repository
        // (and its Scoped DbContext) inside it, and dispose it when done — exactly how
        // an HTTP request scopes its DbContext.
        using var scope = _scopeFactory.CreateScope();
        var ticketRepository =
            scope.ServiceProvider.GetRequiredService<ITicketRepository>();

        var ticket = await ticketRepository.CreateAsync(subject, description);
        return $"Ticket created successfully. Ticket ID: {ticket.Id}. " +
               $"Let the customer know a support agent will follow up.";
    }
}