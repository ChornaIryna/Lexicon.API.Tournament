using System.ComponentModel.DataAnnotations;

namespace Tournament.Shared.DTOs;
public record GameManipulationDto
{
    [Required(ErrorMessage = "The Title is required")]
    public required string Title { get; init; }
    public DateTime Time { get; init; }
}
