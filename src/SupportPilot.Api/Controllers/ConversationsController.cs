using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SupportPilot.Application.Interfaces;

namespace SupportPilot.Api.Controllers;

/// <summary>
/// Admin endpoint for reviewing conversations. List returns lightweight
/// summaries; GetById returns the full transcript for one conversation.
/// </summary>
[EnableRateLimiting("api")]
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class ConversationsController : ControllerBase
{
    private readonly IConversationRepository _conversationRepository;

    public ConversationsController(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;

        var result = await _conversationRepository.GetPagedSummariesAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var conversation = await _conversationRepository.GetByIdAsync(id);
        if (conversation is null) return NotFound();

        return Ok(conversation);
    }
}