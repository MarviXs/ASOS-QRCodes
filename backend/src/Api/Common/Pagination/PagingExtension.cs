namespace Fei.Is.Api.Common.Pagination;

public static class PagingExtension
{
    public static PagedList<T> ToPagedList<T>(this IEnumerable<T> source, int count, int? pageNumber, int? pageSize)
    {
        var items = source.ToList();
        return new PagedList<T>(items, count, pageNumber ?? 1, pageSize.GetValueOrDefault());
    }

    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, PagingParameters pagingParameters)
    {
        var pageNumber = pagingParameters.PageNumber ?? 1;
        return query.Skip((pageNumber - 1) * pagingParameters.PageSize.GetValueOrDefault()).Take(pagingParameters.PageSize.GetValueOrDefault());
    }
}
