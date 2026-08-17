using SupportPilot.Domain.Entities;
using SupportPilot.Domain.Enums;

namespace SupportPilot.Application.Interfaces;

/// <summary>
/// Persists support tickets. In-memory today (InMemoryTicketRepository);
/// a SQL Server-backed implementation will replace it in Week 4 without any
/// change to callers, since they depend on this interface only.
/// </summary>
public interface ITicketRepository
{
    Task<Ticket> CreateAsync(string subject, string description);
    Task<IReadOnlyList<Ticket>> GetAllAsync();
    Task<int> GetTotalCountAsync();
    Task<int> GetCountByStatusAsync(TicketStatus status);
}