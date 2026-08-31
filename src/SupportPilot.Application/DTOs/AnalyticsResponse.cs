namespace SupportPilot.Application.DTOs;

public record DailyMetric(DateOnly Date, int TotalConversations, int EscalatedConversations);

public record SentimentDistribution(int CalmCount, int NeutralCount, int FrustratedCount);

public record AnalyticsResponse(
    IReadOnlyList<DailyMetric> EscalationTrend,
    SentimentDistribution SentimentBreakdown);

public record ConversationAnalyticsRow(DateTime CreatedAt, bool IsEscalated, double? AverageSentimentScore);