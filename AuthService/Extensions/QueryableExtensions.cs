using AuthService.Common.Querying;
using System.Linq.Expressions;

namespace AuthService.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, QueryOptions opt)
    {
        return query
            .Skip((opt.Page - 1) * opt.PageSize)
            .Take(opt.PageSize);
    }

    public static IQueryable<T> ApplyQuery<T>(this IQueryable<T> query, QueryOptions opt, HashSet<string>? whitelist = null)
    {
        foreach (var f in opt.Filter)
        {
            if (whitelist != null && !whitelist.Contains(f.Field))
                continue;

            query = ApplyFilter(query, f);
        }

        query = ApplySort(query, opt, whitelist);

        return query;
    }

    private static IQueryable<T> ApplyFilter<T>(IQueryable<T> query, FilterOption f)
    {
        var param = Expression.Parameter(typeof(T), "x");
        var prop = Expression.PropertyOrField(param, f.Field);

        object? value = Convert.ChangeType(
            f.Value,
            Nullable.GetUnderlyingType(prop.Type) ?? prop.Type);

        var constant = Expression.Constant(value);

        Expression body = f.Op switch
        {
            "eq" => Expression.Equal(prop, constant),
            "contains" => Expression.Call(prop, "Contains", null, constant),
            "gt" => Expression.GreaterThan(prop, constant),
            "lt" => Expression.LessThan(prop, constant),
            _ => Expression.Equal(prop, constant)
        };

        var lambda = Expression.Lambda<Func<T, bool>>(body, param);
        return query.Where(lambda);
    }

    private static IQueryable<T> ApplySort<T>(IQueryable<T> query, QueryOptions opt, HashSet<string>? whitelist)
    {
        bool first = true;

        foreach (var s in opt.Sort)
        {
            if (whitelist != null && !whitelist.Contains(s.Field))
                continue;

            var param = Expression.Parameter(typeof(T), "x");
            var prop = Expression.PropertyOrField(param, s.Field);
            var lambda = Expression.Lambda(prop, param);

            string method = first
                ? (s.Desc ? "OrderByDescending" : "OrderBy")
                : (s.Desc ? "ThenByDescending" : "ThenBy");

            query = (IQueryable<T>)typeof(Queryable)
                .GetMethods()
                .First(m => m.Name == method && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), prop.Type)
                .Invoke(null, [query, lambda])!;

            first = false;
        }

        return query;
    }
}