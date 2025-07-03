namespace Tournament.Shared.Responses;
public class ApiError
{
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
}