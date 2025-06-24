namespace Tournament.Core.DTOs;
public record GameManipulationDto
{
    public required string Title { get; set; }
    public DateTime Time { get; set; }
}
