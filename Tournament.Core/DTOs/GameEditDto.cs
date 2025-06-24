namespace Tournament.Core.DTOs;
public record GameEditDto : GameManipulationDto
{
    public int Id { get; init; }
}
