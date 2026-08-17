using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportPilot.Application.DTOs;
using SupportPilot.Application.Interfaces;

namespace SupportPilot.Api.Controllers;

/// <summary>
/// Admin endpoint for managing knowledge base articles directly in the
/// vector store. Add embeds and stores new text; list/delete let the admin
/// review and prune what the agent's search_kb tool can retrieve.
/// </summary>
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class KnowledgeBaseController : ControllerBase
{
    private readonly IKnowledgeBaseService _knowledgeBaseService;
    private readonly ILogger<KnowledgeBaseController> _logger;

    public KnowledgeBaseController(
        IKnowledgeBaseService knowledgeBaseService,
        ILogger<KnowledgeBaseController> logger)
    {
        _knowledgeBaseService = knowledgeBaseService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int limit = 100)
    {
        var articles = await _knowledgeBaseService.GetAllAsync(limit);
        return Ok(articles);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddArticleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Text cannot be empty.");
        }

        var article = await _knowledgeBaseService.AddArticleAsync(request.Text);

        _logger.LogInformation("Knowledge base article added. Id: {Id}", article.Id);

        return Ok(article);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _knowledgeBaseService.DeleteArticleAsync(id);

        _logger.LogInformation("Knowledge base article deleted. Id: {Id}", id);

        return NoContent();
    }
}