namespace SupportPilot.Domain.Enums;

/// <summary>
/// Access level for an authenticated user. Named "UserRole" (not "Role") to
/// avoid future collisions with unrelated role-type concepts elsewhere in
/// the domain.
/// </summary>
public enum UserRole
{
    Customer,
    Agent,
    Admin
}