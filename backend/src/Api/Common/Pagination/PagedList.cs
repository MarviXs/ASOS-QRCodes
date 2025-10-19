using System.Text.Json.Serialization;

namespace Fei.Is.Api.Common.Pagination;

public class PagedList<T>
{
    public int CurrentPage { get; }
    public int TotalPages { get; }
    public int PageSize { get; }
    public int TotalCount { get; }

    public bool HasPrevious => CurrentPage > 1;

    public bool HasNext => CurrentPage < TotalPages;
    public List<T> Items { get; } = [];

    public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        Items = items.ToList();
    }

    public PagedList() { }

    // Constructor for deserialization in tests
    [JsonConstructor]
    public PagedList(int currentPage, int totalPages, int pageSize, int totalCount, List<T> items)
    {
        CurrentPage = currentPage;
        TotalPages = totalPages;
        PageSize = pageSize;
        TotalCount = totalCount;
        Items = items ?? [];
    }
}
