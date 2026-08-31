using SupportPilot.Application.DTOs;
using SupportPilot.Application.Interfaces;
using SupportPilot.Domain.Enums;

namespace SupportPilot.Application.Services;

public class MetricsService : IMetricsService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ITicketRepository _ticketRepository;

    public MetricsService(
        IConversationRepository conversationRepository,
        ITicketRepository ticketRepository)
    {
        _conversationRepository = conversationRepository;
        _ticketRepository = ticketRepository;
    }

    public async Task<MetricsResponse> GetMetricsAsync()
    {
        int totalConversations = await _conversationRepository.GetTotalCountAsync();
        int escalatedConversations = await _conversationRepository.GetEscalatedCountAsync();
        double? averageSentiment = await _conversationRepository.GetAverageSentimentScoreAsync();

        int totalTickets = await _ticketRepository.GetTotalCountAsync();
        int openTickets = await _ticketRepository.GetCountByStatusAsync(TicketStatus.Open);

        double escalationRate = totalConversations == 0
            ? 0
            : (double)escalatedConversations / totalConversations;

        return new MetricsResponse(
            totalConversations,
            escalatedConversations,
            escalationRate,
            totalTickets,
            openTickets,
            averageSentiment);
    }

    public async Task<AnalyticsResponse> GetAnalyticsAsync()
    {
        var sinceUtc = DateTime.UtcNow.Date.AddDays(-6); // last 7 days including today
        var rows = await _conversationRepository.GetConversationsForAnalyticsAsync(sinceUtc);

        var trend = Enumerable.Range(0, 7)
            .Select(offset => sinceUtc.AddDays(offset))
            .Select(day =>
            {
                var dayRows = rows.Where(r => r.CreatedAt.Date == day.Date).ToList();
                return new DailyMetric(
                    DateOnly.FromDateTime(day),
                    dayRows.Count,
                    dayRows.Count(r => r.IsEscalated));
            })
            .ToList();

        int calm = rows.Count(r => r.AverageSentimentScore is >= 0 and < 0.3);
        int neutral = rows.Count(r => r.AverageSentimentScore is >= 0.3 and < 0.7);
        int frustrated = rows.Count(r => r.AverageSentimentScore is >= 0.7);

        var distribution = new SentimentDistribution(calm, neutral, frustrated);

        return new AnalyticsResponse(trend, distribution);
    }
}