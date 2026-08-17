using Microsoft.AspNetCore.Mvc;
using SupportPilot.Application.Interfaces;

namespace SupportPilot.Api.Controllers;

/// <summary>
/// Temporary endpoints used to verify infrastructure wiring during development.
/// Removed once the real chat and knowledge base endpoints are in place.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly IChatProvider _chatProvider;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(
        IChatProvider chatProvider,
        IEmbeddingService embeddingService,
        IVectorStore vectorStore,
        ILogger<DiagnosticsController> logger)
    {
        _chatProvider = chatProvider;
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _logger = logger;
    }

    /// <summary>
    /// Verifies the full vector pipeline: embed text, store it, then search it back.
    /// </summary>
    [HttpGet("vector-roundtrip")]
    public async Task<IActionResult> VectorRoundTrip()
    {
        const string sampleText = "SupportPilot handles customer support tickets automatically.";
        const string query = "What does SupportPilot do?";

        // [1] Make sure the collection exists before writing to it.
        await _vectorStore.EnsureCollectionExistsAsync();

        // [2] Embed and store the sample text.
        float[] sampleVector = await _embeddingService.GetEmbeddingAsync(sampleText);
        await _vectorStore.UpsertTextAsync(sampleText, sampleVector);

        // [3] Embed the query and search for the closest stored text.
        float[] queryVector = await _embeddingService.GetEmbeddingAsync(query);
        var results = await _vectorStore.SearchAsync(queryVector, limit: 3);

        _logger.LogInformation("Vector round-trip completed. Results: {Count}", results.Count);

        return Ok(new
        {
            provider = _chatProvider.Name,
            model = _chatProvider.ModelName,
            embeddingDimensions = sampleVector.Length,
            resultCount = results.Count,
            results
        });
    }
}