namespace AuthService.Common.Querying;

public static class QueryParser
{
    public static QueryOptions Parse(IQueryCollection query)
    {
        var opt = new QueryOptions();

        if (query.TryGetValue("search", out var search))
            opt.Search = search;

        if (query.TryGetValue("page", out var page))
            opt.Page = int.Parse(page!);

        if (query.TryGetValue("pageSize", out var size))
            opt.PageSize = int.Parse(size!);

        if (query.TryGetValue("sort", out var sorts))
        {
            foreach (var s in sorts)
            {
                if (string.IsNullOrWhiteSpace(s))
                    continue;

                var parts = s.Split(':', StringSplitOptions.TrimEntries);

                if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0]))
                    continue;

                opt.Sort.Add(new SortOption
                {
                    Field = parts[0],
                    Desc = parts.Length > 1 &&
                           parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase)
                });
            }
        }

        if (query.TryGetValue("filter", out var filters))
        {
            foreach (var f in filters)
            {
                if (string.IsNullOrWhiteSpace(f))
                    continue;

                var p = f.Split('~', StringSplitOptions.TrimEntries);

                if (p.Length != 3)
                    continue;

                if (string.IsNullOrWhiteSpace(p[0]) ||
                    string.IsNullOrWhiteSpace(p[1]))
                    continue;

                opt.Filter.Add(new FilterOption
                {
                    Field = p[0],
                    Op = p[1].ToLowerInvariant(),
                    Value = p[2]
                });
            }
        }

        return opt;
    }
}