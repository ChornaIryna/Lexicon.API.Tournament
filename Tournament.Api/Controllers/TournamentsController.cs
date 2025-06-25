using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Tournament.Core.DTOs;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;

namespace Tournament.Api.Controllers;

[AllowAnonymous] // Allow anonymous access to the API for demonstration purposes; adjust as needed for security.
[Route("api/[controller]")]
[ApiController]
public class TournamentsController(IMapper mapper, IUoW unitOfWork) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TournamentDto>>> GetTournaments(bool includeGames = false)
    {
        var tournaments = await unitOfWork.TournamentRepository.GetAllTournamentsAsync(includeGames);
        if (tournaments == null || !tournaments.Any())
        {
            return NotFound("No tournaments found.");
        }
        var tournamentsDto = includeGames ? mapper.Map<IEnumerable<TournamentWithGamesDto>>(tournaments) : mapper.Map<IEnumerable<TournamentDto>>(tournaments);
        return Ok(tournamentsDto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TournamentDto>> GetTournamentDetails(int id, bool includeGames = false)
    {
        var tournamentDetails = await unitOfWork.TournamentRepository.GetTournamentByIdAsync(id);

        if (tournamentDetails == null)
        {
            return NotFound($"Tournament with Id '{id}' was not found.");
        }
        var tournamentDto = includeGames ? mapper.Map<TournamentWithGamesDto>(tournamentDetails) : mapper.Map<TournamentDto>(tournamentDetails);
        return Ok(tournamentDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutTournamentDetails(int id, TournamentEditDto tournamentDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != tournamentDto.Id
            && !await unitOfWork.TournamentRepository.AnyAsync(id))
            return BadRequest();

        var tournamentDetails = await unitOfWork.TournamentRepository.GetTournamentByIdAsync(id);
        if (tournamentDetails == null)
            return NotFound($"Tournament with id '{id}' was not found");
        mapper.Map(tournamentDto, tournamentDetails);

        var validationContext = new ValidationContext(tournamentDetails, serviceProvider: null, items: null);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(tournamentDetails, validationContext, validationResults, true))
        {
            foreach (var validationResult in validationResults)
            {
                var errorMessage = validationResult.ErrorMessage ?? "Unknown validation error.";
                ModelState.AddModelError(validationResult.MemberNames.FirstOrDefault() ?? string.Empty, errorMessage);
            }
            return BadRequest(ModelState);
        }

        try
        {
            unitOfWork.TournamentRepository.Update(tournamentDetails);
            await unitOfWork.CompleteAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (!await unitOfWork.TournamentRepository.AnyAsync(id))
                return NotFound($"Tournament with id '{id}' was not found");
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<TournamentDto>> PostTournamentDetails(TournamentCreateDto tournamentDto)
    {
        var tournament = mapper.Map<TournamentDetails>(tournamentDto);

        var validationContext = new ValidationContext(tournament, serviceProvider: null, items: null);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(tournament, validationContext, validationResults, true))
        {
            foreach (var validationResult in validationResults)
            {
                var errorMessage = validationResult.ErrorMessage ?? "Unknown validation error.";
                ModelState.AddModelError(validationResult.MemberNames.FirstOrDefault() ?? string.Empty, errorMessage);
            }
            return BadRequest(ModelState);
        }

        unitOfWork.TournamentRepository.Add(tournament);
        try
        {
            await unitOfWork.CompleteAsync();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }

        var createdTournamentDto = mapper.Map<TournamentDto>(tournament);
        return CreatedAtAction(nameof(GetTournamentDetails), new { id = tournament.Id }, createdTournamentDto);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> PatchTournamentDetails(int id, JsonPatchDocument<TournamentEditDto> patchDoc)
    {
        if (patchDoc == null)
            return BadRequest("Patch document cannot be null.");

        var tournamentDetails = await unitOfWork.TournamentRepository.GetTournamentByIdAsync(id);
        if (tournamentDetails == null)
            return NotFound($"Tournament with id '{id}' was not found");

        var tournamentEditDto = mapper.Map<TournamentEditDto>(tournamentDetails);
        patchDoc.ApplyTo(tournamentEditDto, ModelState);
        TryValidateModel(tournamentEditDto);
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);

        mapper.Map(tournamentEditDto, tournamentDetails);
        try
        {
            await unitOfWork.CompleteAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (!await unitOfWork.TournamentRepository.AnyAsync(id))
                return NotFound($"Tournament with id '{id}' was not found");
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTournamentDetails(int id)
    {
        var tournament = await unitOfWork.TournamentRepository.GetTournamentByIdAsync(id);
        if (tournament == null)
            return NotFound($"Tournament with id '{id}' was not found");

        unitOfWork.TournamentRepository.Remove(tournament);
        try
        {
            await unitOfWork.CompleteAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }

        return NoContent();
    }
}
