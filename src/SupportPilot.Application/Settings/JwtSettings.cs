namespace SupportPilot.Application.Settings;

/// <summary>
/// JWT signing configuration, bound from appsettings + User Secrets
/// ("Jwt" section). Key is never committed to source control.
/// </summary>
public class JwtSettings
{
    public required string Key { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public int ExpiryMinutes { get; init; } = 60;
}