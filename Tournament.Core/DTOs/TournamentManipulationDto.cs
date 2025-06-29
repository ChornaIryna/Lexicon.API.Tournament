﻿namespace Tournament.Core.DTOs;
public record TournamentManipulationDto
{
    public required string Title { get; init; }

    public DateTime StartDate { get; init; }
}
