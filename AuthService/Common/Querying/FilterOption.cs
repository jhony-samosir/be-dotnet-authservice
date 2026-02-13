namespace AuthService.Common.Querying;

public class FilterOption
{
    public string Field { get; init; } = "";
    public string Op { get; init; } = "eq";
    public string Value { get; init; } = "";
}
