namespace Tournament.Shared.DTOs;
public record TournamentCreateDto : TournamentManipulationDto
{
    public IEnumerable<GameDto>? Games { get; init; }
}
