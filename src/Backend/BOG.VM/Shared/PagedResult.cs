namespace BOG.VM.Shared;

/// <summary>
/// Generic paged result wrapper for API responses.
/// Used for all paginated list endpoints.
/// </summary>
/// <typeparam name="T">Type of items in the result list</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// List of items for the current page
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Calculated total number of pages
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Index of the first item on this page (1-based)
    /// </summary>
    public int FirstItemIndex => PageSize > 0 ? ((PageNumber - 1) * PageSize) + 1 : 0;

    /// <summary>
    /// Index of the last item on this page (1-based)
    /// </summary>
    public int LastItemIndex => Math.Min(PageNumber * PageSize, TotalCount);

    /// <summary>
    /// Creates a new paged result
    /// </summary>
    public PagedResult()
    {
    }

    /// <summary>
    /// Creates a new paged result with parameters
    /// </summary>
    public PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items ?? new();
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
