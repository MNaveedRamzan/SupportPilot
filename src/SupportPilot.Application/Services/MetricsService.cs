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
}