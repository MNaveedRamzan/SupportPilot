using SupportPilot.Domain.Entities;

namespace SupportPilot.Application.Contracts;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
}