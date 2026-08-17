using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportPilot.Application.Interfaces;
using SupportPilot.Domain.Entities;
using SupportPilot.Domain.Enums;
using SupportPilot.Infrastructure.Persistence;

namespace SupportPilot.Infrastructure.Tickets;

/// <summary>
/// PostgreSQL-backed ticket store using EF Core. Replaces InMemoryTicketRepository
/// as the registered ITicketRepository — callers are unaffected since they depend
/// on the interface only (Dependency Inversion). Tickets now survive app restarts.
/// </summary>
public class EfTicketRepository : ITicketRepository
{
    private readonly SupportPilotDbContext _dbContext;
    private readonly ILogger<EfTicketRepository> _logger;

    public EfTicketRepository(
        SupportPilotDbContext dbContext,
        ILogger<EfTicketRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Ticket> CreateAsync(string subject, string description)
    {
        var ticket = new Ticket
        {
            Subject = subject,
            Description = description
        };

        _dbContext.Tickets.Add(ticket);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Ticket persisted. Id: {TicketId}, Subject: {Subject}",
            ticket.Id, ticket.Subject);

        return ticket;
    }

    public async Task<IReadOnlyList<Ticket>> GetAllAsync()
    {
        return await _dbContext.Tickets
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _dbContext.Tickets.CountAsync();
    }

    public async Task<int> GetCountByStatusAsync(TicketStatus status)
    {
        return await _dbContext.Tickets.CountAsync(t => t.Status == status);
    }
}