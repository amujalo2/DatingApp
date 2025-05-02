namespace API.Helpers;
public class PaginationHeader(int currentPage, int itemsPerPage, int totalitems, int totalPages)
{
    public int CurrentPage { get; set; } = currentPage;
    public int ItemsPerPage { get; set; } = itemsPerPage;
    public int TotalItems { get; set; } = totalitems;
    public int TotalPages { get; set; } = totalPages;

}