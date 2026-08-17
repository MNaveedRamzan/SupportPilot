using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SupportPilot.Application.DTOs;

namespace SupportPilot.Api.Controllers;

/// <summary>
/// Agentic endpoint: the LLM decides on its own, based on the conversation,
/// whether to search the knowledge base, create a support ticket, both, or
/// neither. Unlike ChatController (deterministic retrieve-then-answer), this
/// endpoint lets the model reason about which tools to invoke.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly Kernel _kernel;
    private readonly ILogger<AgentController> _logger;

    public AgentController(Kernel kernel, ILogger<AgentController> logger)
    {
        _kernel = kernel;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Ask([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Message cannot be empty.");
        }

        var chatService = _kernel.GetRequiredService<IChatCompletionService>();

        var history = new ChatHistory();
        history.AddSystemMessage(
            "You are a customer support assistant. Use search_kb to find information " +
            "before answering policy questions. If the knowledge base does not have the " +
            "answer, or the customer asks for a human, use create_ticket.");
        history.AddUserMessage(request.Message);

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        _logger.LogInformation("Agent request received. MessageLength: {Length}",
            request.Message.Length);

        ChatMessageContent result = await chatService.GetChatMessageContentAsync(
            history, executionSettings, _kernel);

        return Ok(new { response = result.Content });
    }
}