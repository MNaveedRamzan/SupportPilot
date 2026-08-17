using Microsoft.AspNetCore.Mvc;

namespace SupportPilot.Api.Controllers;

/// <summary>
/// Simple health-check endpoint to verify the API is running.
/// Confirms the scaffold is wired correctly before feature work begins.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            service = "SupportPilot.Api",
            timestamp = DateTime.UtcNow
        });
    }
}