namespace SupportPilot.Application.DTOs;

public record MetricsResponse(
    int TotalConversations,
    int EscalatedConversations,
    double EscalationRate,
    int TotalTickets,
    int OpenTickets,
    double? AverageSentimentScore);