namespace Tournament.Core.DTOs;
public record EditTournamentDto
{
    public required string Title { get; init; }

    public DateTime StartDate { get; init; } = DateTime.UtcNow;
}
