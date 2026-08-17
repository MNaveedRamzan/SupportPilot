using Microsoft.AspNetCore.Mvc;
using SupportPilot.Application.DTOs;
using SupportPilot.Application.Interfaces;

namespace SupportPilot.Api.Controllers;

/// <summary>
/// Chat endpoint backed by retrieval-augmented generation. Answers are grounded
/// in the knowledge base; questions with no relevant context are declined rather
/// than answered from the model's own knowledge.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IRagService _ragService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IRagService ragService, ILogger<ChatController> logger)
    {
        _ragService = ragService;
        _logger = logger;
    }

    /// <summary>
    /// Answers a question using the knowledge base.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ChatAnswer), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Ask([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Message cannot be empty.");
        }

        _logger.LogInformation("Chat request received. MessageLength: {Length}",
            request.Message.Length);

        ChatAnswer answer = await _ragService.AskAsync(request);

        return Ok(answer);
    }
}