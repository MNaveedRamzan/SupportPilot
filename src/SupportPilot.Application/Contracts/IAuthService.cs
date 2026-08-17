using SupportPilot.Application.DTOs;

namespace SupportPilot.Application.Contracts;

public interface IAuthService
{
    /// <summary>Returns null if the email is already registered.</summary>
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);

    /// <summary>Returns null if the email doesn't exist or the password is wrong.</summary>
    Task<AuthResponse?> LoginAsync(LoginRequest request);
}