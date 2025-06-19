namespace Tournament.Core.DTOs;
public record GameDto
{
    public required string Title { get; init; }
    public DateTime Time { get; init; }
}
