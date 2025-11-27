namespace WebApplication1.ViewModels;

public class PaginationViewModel<T>
{
    public List<T> Items { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int PageSize { get; set; } = 12; // Items per page
    public int TotalItems { get; set; }
    
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    
    public PaginationViewModel()
    {
    }
    
    public PaginationViewModel(List<T> items, int currentPage, int pageSize, int totalItems)
    {
        Items = items;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalItems = totalItems;
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
    }
}
