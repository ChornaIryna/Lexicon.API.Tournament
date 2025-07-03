namespace Tournament.Shared.DTOs;
public record GameEditDto : GameManipulationDto
{
    public int Id { get; init; }
}
