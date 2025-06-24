namespace Tournament.Core.DTOs;
public record TournamentEditDto : TournamentManipulationDto
{
    public int Id { get; init; }
}
