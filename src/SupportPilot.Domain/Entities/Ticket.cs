using SupportPilot.Domain.Enums;

namespace SupportPilot.Domain.Entities;

/// <summary>
/// A support ticket created when the agent cannot resolve a question from the
/// knowledge base and escalation is needed. Has identity (Id) — two tickets are
/// never the same even with identical content, which is why this is an Entity
/// rather than a Common value type.
/// </summary>
public class Ticket
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Subject { get; init; }
    public required string Description { get; init; }
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}