using Microsoft.Extensions.Options;
using SupportPilot.Application.Contracts;
using SupportPilot.Application.DTOs;
using SupportPilot.Application.Settings;
using SupportPilot.Domain.Entities;
using SupportPilot.Domain.Enums;

namespace SupportPilot.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthService(IUserRepository userRepository, IJwtTokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email);
        if (existing is not null)
        {
            return null;
        }

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Customer
        };

        await _userRepository.AddAsync(user);

        var token = _tokenGenerator.GenerateToken(user);
        return new AuthResponse(token, user.Email, user.Role.ToString());
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var token = _tokenGenerator.GenerateToken(user);
        return new AuthResponse(token, user.Email, user.Role.ToString());
    }
}