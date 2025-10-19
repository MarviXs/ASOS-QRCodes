namespace Fei.Is.Api.Common.Pagination;

public class SearchParameters : PagingParameters
{
    public string? SortBy { get; set; }
    public bool? Descending { get; set; }
    public string? SearchTerm { get; set; }
}
