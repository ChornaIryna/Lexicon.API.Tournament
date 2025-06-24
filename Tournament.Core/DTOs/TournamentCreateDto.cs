namespace Tournament.Core.DTOs;
public record TournamentCreateDto : TournamentManipulationDto
{
    public IEnumerable<GameDto>? Games { get; init; }
}
