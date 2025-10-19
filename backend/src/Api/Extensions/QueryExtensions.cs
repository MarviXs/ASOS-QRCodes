using System.Linq.Dynamic.Core;
using Fei.Is.Api.Common.Utils;

namespace Fei.Is.Api.Extensions;

public static class QueryExtensions
{
    public static IQueryable<T> Sort<T>(this IQueryable<T> query, string? sortBy, bool? descending)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return query;
        }

        bool isDescending = descending.HasValue && descending.Value;
        string sortDirection = isDescending ? "desc" : "asc";

        return query.OrderBy($"{sortBy} {sortDirection}");
    }

    public static IQueryable<T> Sort<T, TKey>(this IQueryable<T> source, Func<T, TKey> selector, bool? descending)
    {
        if (!descending ?? false)
        {
            return (IQueryable<T>)source.OrderBy(selector);
        }
        else
        {
            return (IQueryable<T>)source.OrderByDescending(selector);
        }
    }
}
