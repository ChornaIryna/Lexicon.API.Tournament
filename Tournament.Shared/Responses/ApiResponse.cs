namespace Tournament.Shared.Responses;
public class ApiResponse<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public IEnumerable<string?> Errors { get; set; } = [];
    public int Status { get; set; }
    public object? MetaData { get; set; }
}