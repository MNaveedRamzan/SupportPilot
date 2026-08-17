using SupportPilot.Domain.Enums;

namespace SupportPilot.Domain.Entities;

/// <summary>
/// An account holder in the system. PasswordHash stores a BCrypt hash, never
/// plaintext. Email is enforced unique at the DB level (see DbContext),
/// not just in application code, to avoid race conditions on concurrent
/// registration attempts.
/// </summary>
public class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Email { get; init; }
    public required string PasswordHash { get; set; }
    public UserRole Role { get; set; } = UserRole.Customer;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}