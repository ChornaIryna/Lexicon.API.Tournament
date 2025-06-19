using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
    public async Task<ActionResult<IEnumerable<TournamentDto>>> GetTournamentDetails()
    {
        var tournaments = await unitOfWork.TournamentRepository.GetAllTournamentsAsync();
        if (tournaments == null || !tournaments.Any())
        {
            return NotFound("No tournaments found.");
        }
        var tournamentsDto = mapper.Map<IEnumerable<TournamentDto>>(tournaments);
        return Ok(tournamentsDto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TournamentDto>> GetTournamentDetails(int id)
    {
        var tournamentDetails = await unitOfWork.TournamentRepository.GetTournamentByIdAsync(id);

        if (tournamentDetails == null)
        {
            return NotFound();
        }
        var tournamentDto = mapper.Map<TournamentDto>(tournamentDetails);
        return Ok(tournamentDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutTournamentDetails(int id, EditTournamentDto tournamentDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != mapper.Map<TournamentDetails>(tournamentDto).Id
            && !unitOfWork.TournamentRepository.AnyAsync(id).Result)
            return BadRequest();

        var tournamentDetails = await unitOfWork.TournamentRepository.GetTournamentByIdAsync(id);
        if (tournamentDetails == null)
            return NotFound();
        mapper.Map(tournamentDto, tournamentDetails);

        var validationContext = new ValidationContext(tournamentDetails, serviceProvider: null, items: null);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(tournamentDetails, validationContext, validationResults, true))
        {
            foreach (var validationResult in validationResults)
            {
                ModelState.AddModelError(validationResult.MemberNames.FirstOrDefault()
                    ?? string.Empty, validationResult.ErrorMessage);
            }
            return BadRequest(ModelState);
        }

        try
        {
            unitOfWork.TournamentRepository.Update(tournamentDetails);
            await unitOfWork.CompleteAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await TournamentDetailsExists(id))
                return NotFound();
            else
                return StatusCode(500);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return StatusCode(500);
        }

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<TournamentDto>> PostTournamentDetails(EditTournamentDto tournamentDto)
    {
        var tournament = mapper.Map<TournamentDetails>(tournamentDto);

        var validationContext = new ValidationContext(tournament, serviceProvider: null, items: null);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(tournament, validationContext, validationResults, true))
        {
            foreach (var validationResult in validationResults)
            {
                ModelState.AddModelError(validationResult.MemberNames.FirstOrDefault()
                    ?? string.Empty, validationResult.ErrorMessage);
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
            Console.WriteLine(ex.ToString());
            return StatusCode(500);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return StatusCode(500);
        }

        var createdTournamentDto = mapper.Map<TournamentDto>(tournament);
        return CreatedAtAction(nameof(GetTournamentDetails), new { id = tournament.Id }, createdTournamentDto);
    }


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTournamentDetails(int id)
    {
        var tournament = await unitOfWork.TournamentRepository.GetTournamentByIdAsync(id);
        if (tournament == null)
            return NotFound();

        unitOfWork.TournamentRepository.Remove(tournament);
        try
        {
            await unitOfWork.CompleteAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return StatusCode(500);
        }

        return NoContent();
    }

    private async Task<bool> TournamentDetailsExists(int id)
    {
        return await unitOfWork.TournamentRepository.AnyAsync(id);
    }
}
