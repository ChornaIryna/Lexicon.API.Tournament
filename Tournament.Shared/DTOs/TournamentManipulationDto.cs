using System.ComponentModel.DataAnnotations;

namespace Tournament.Shared.DTOs;
public record TournamentManipulationDto
{
    [Required(ErrorMessage = "The Title is required.")]
    public required string Title { get; init; }

    public DateTime StartDate { get; init; }
}
