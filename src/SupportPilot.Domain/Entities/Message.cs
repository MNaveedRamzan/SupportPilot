using SupportPilot.Domain.Enums;

namespace SupportPilot.Domain.Entities;

/// <summary>
/// A single turn in a conversation. SentimentScore is only populated for
/// User-role messages (0.0 = calm, 1.0 = highly frustrated) — assistant
/// replies are never sentiment-scored.
/// </summary>
public class Message
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ConversationId { get; init; }
    public ChatRole Role { get; init; }
    public required string Content { get; init; }
    public float? SentimentScore { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}