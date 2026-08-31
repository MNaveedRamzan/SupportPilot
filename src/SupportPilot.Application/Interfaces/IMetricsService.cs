using SupportPilot.Application.DTOs;

namespace SupportPilot.Application.Interfaces;

/// <summary>
/// Combines conversation and ticket data into dashboard-level metrics.
/// Lives in Application (not the controller) because computing escalation
/// rate is business logic, not an HTTP concern.
/// </summary>
public interface IMetricsService
{
    Task<MetricsResponse> GetMetricsAsync();
    Task<AnalyticsResponse> GetAnalyticsAsync();
}