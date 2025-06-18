namespace Tournament.Core.Entities;
public class TournamentDetails
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public DateTime StartDate { get; set; }
    public ICollection<Games>? Games { get; set; }
}
