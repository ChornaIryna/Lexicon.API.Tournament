namespace Tournament.Core.DTOs;
public record TournamentWithGamesDto : TournamentDto
{
    public IEnumerable<GameDto>? Games { get; init; }
}
