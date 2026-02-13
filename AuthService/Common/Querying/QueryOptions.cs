namespace AuthService.Common.Querying;

public class QueryOptions
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }

    public List<SortOption> Sort { get; set; } = [];
    public List<FilterOption> Filter { get; set; } = [];
}