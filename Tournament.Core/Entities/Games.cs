namespace Tournament.Core.Entities;

public class Games
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public DateTime Time { get; set; }
    public int TournamentId { get; set; }
}