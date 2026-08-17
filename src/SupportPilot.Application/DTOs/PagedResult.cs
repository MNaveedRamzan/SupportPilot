namespace SupportPilot.Application.DTOs;

/// <summary>
/// Generic offset-pagination envelope. Reused for any paged list endpoint.
/// </summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);