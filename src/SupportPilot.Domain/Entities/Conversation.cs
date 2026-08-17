namespace SupportPilot.Domain.Entities;

/// <summary>
/// A chat session between a customer and the assistant. Owns the full message
/// history (unlike the earlier stateless ChatHub calls) so sentiment can be
/// tracked per turn and conversations can be reviewed later in the admin
/// dashboard (Week 4, Day 3-4).
/// </summary>
public class Conversation
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// True once this conversation has triggered an escalation. Prevents
    /// creating duplicate tickets for the same conversation on later messages.
    /// </summary>
    public bool IsEscalated { get; set; }

    /// <summary>
    /// The ticket created when this conversation was escalated. Not a hard
    /// foreign key — Ticket and Conversation are separate concerns, this is
    /// just a reference for navigation.
    /// </summary>
    public Guid? LinkedTicketId { get; set; }

    public List<Message> Messages { get; init; } = new();
}