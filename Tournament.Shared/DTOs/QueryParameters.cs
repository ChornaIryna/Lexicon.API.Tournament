namespace Tournament.Shared.DTOs;
public record QueryParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public int PageNumber { get; set; } = 1;
    public string? OrderBy { get; set; }
    public string? SearchTerm { get; set; }

    public bool IsValid() => PageNumber > 0 && PageSize > 0;
}
