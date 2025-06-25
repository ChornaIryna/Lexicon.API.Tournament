namespace Tournament.Core.DTOs;
public record GameManipulationDto
{
    public required string Title { get; init; }
    public DateTime Time { get; init; }
}
