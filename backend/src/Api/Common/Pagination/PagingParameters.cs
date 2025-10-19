namespace Fei.Is.Api.Common.Pagination;

public abstract class PagingParameters
{
    const int maxPageSize = 500;

    private int _pageSize = 20;

    public int? PageNumber { get; set; }

    public int? PageSize
    {
        get { return _pageSize; }
        set { _pageSize = value.HasValue ? (value > maxPageSize ? maxPageSize : value.Value) : _pageSize; }
    }
}
