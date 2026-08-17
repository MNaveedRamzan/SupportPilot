using SupportPilot.Domain.Entities;

namespace SupportPilot.Application.Contracts;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}