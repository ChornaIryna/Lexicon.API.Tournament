using System.ComponentModel.DataAnnotations;

namespace Tournament.Core.Entities;

public class Game
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public required string Title { get; set; }
    public DateTime Time { get; set; }
    public int TournamentDetailsId { get; set; }
}