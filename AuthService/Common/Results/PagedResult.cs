using System.Text.Json.Serialization;

namespace AuthService.Common.Results;

/// <summary>
/// Paged list result with total count for list endpoints.
/// </summary>
public record PagedResult<T>(
    [property: JsonPropertyName("items")] IReadOnlyList<T> Items,
    [property: JsonPropertyName("total_count")] int TotalCount,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("page_size")] int PageSize
);
