using Microsoft.EntityFrameworkCore;
using SupportPilot.Application.Contracts;
using SupportPilot.Domain.Entities;

namespace SupportPilot.Infrastructure.Persistence;

public class EfUserRepository : IUserRepository
{
    private readonly SupportPilotDbContext _context;

    public EfUserRepository(SupportPilotDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByEmailAsync(string email) =>
        _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }
}