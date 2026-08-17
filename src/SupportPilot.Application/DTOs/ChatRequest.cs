namespace SupportPilot.Application.DTOs;

/// <summary>
/// Incoming chat request from a client.
/// Conversation history will be added here once session handling is in place.
/// </summary>
public record ChatRequest(string Message);