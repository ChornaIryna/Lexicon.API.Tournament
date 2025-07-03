namespace Tournament.Shared.DTOs;
public record TournamentEditDto : TournamentManipulationDto
{
    public int Id { get; init; }
}
