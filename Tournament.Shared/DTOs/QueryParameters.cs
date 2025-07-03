namespace Tournament.Shared.DTOs;
public class QueryParameters
{
    public string? OrderBy { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
