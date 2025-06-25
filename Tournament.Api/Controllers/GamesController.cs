using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tournament.Core.DTOs;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;

namespace Tournament.Api.Controllers;
[AllowAnonymous]
[Route("api/tournaments/{tournamentId:int}/[controller]")]
[ApiController]
public class GamesController(IMapper mapper, IUoW unitOfWork) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameDto>>> GetGames(int tournamentId, [FromQuery] QueryParameters queryParameters)
    {
        if (!await unitOfWork.TournamentRepository.AnyAsync(tournamentId))
            return NotFound($"Tournament with Id '{tournamentId}' was not found.");

        var games = !string.IsNullOrEmpty(queryParameters.SearchTerm)
            ? await unitOfWork.GameRepository.GetGamesByTitleAsync(queryParameters.SearchTerm)
            : await unitOfWork.GameRepository.GetAllAsync(tournamentId);

        if (games == null || !games.Any())
            return NotFound("No games found for the specified tournament.");

        if (!string.IsNullOrEmpty(queryParameters.OrderBy))
        {
            games = queryParameters.OrderBy.ToLower() switch
            {
                "title" => games.OrderBy(g => g.Title ?? string.Empty),
                "time" => games.OrderBy(g => g.Time),
                _ => games
            };
        }

        var paginatedGames = games
            .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize)
            .Take(queryParameters.PageSize);

        return Ok(mapper.Map<IEnumerable<GameDto>>(paginatedGames));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GameDto>> GetGame(int tournamentId, int id)
    {
        if (!await unitOfWork.TournamentRepository.AnyAsync(tournamentId))
            return NotFound($"Tournament with Id '{tournamentId}' was not found.");

        var game = await unitOfWork.GameRepository.GetByIdAsync(id);
        if (game == null)
            return NotFound($"Game with Id '{id}' was not found.");

        if (game.TournamentDetailsId != tournamentId)
            return BadRequest($"Game with id '{id}' does not belong to tournament with Id '{tournamentId}'.");

        return Ok(mapper.Map<GameDto>(game));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutGame(int tournamentId, int id, GameEditDto game)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!await unitOfWork.TournamentRepository.AnyAsync(tournamentId))
            return NotFound($"Tournament with Id '{tournamentId}' was not found.");

        if (id != game.Id)
            return BadRequest("Game ID mismatch.");

        var existingGame = await unitOfWork.GameRepository.GetByIdAsync(id);
        if (existingGame == null)
            return NotFound($"Game with id '{id}' does not exist.");

        if (existingGame.TournamentDetailsId != tournamentId)
            return BadRequest($"Game with id '{id}' does not belong to tournament with Id '{tournamentId}'.");

        mapper.Map(game, existingGame);
        try
        {
            if (unitOfWork.HasChanges())
                await unitOfWork.CompleteAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (!await unitOfWork.GameRepository.AnyAsync(id))
                return NotFound($"Game with id '{id}' does not exist.");

            return Conflict(new
            {
                Message = "Concurrency error occurred while updating the game.",
                Details = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<Game>> PostGames(int tournamentId, GameCreateDto gameDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!await unitOfWork.TournamentRepository.AnyAsync(tournamentId))
            return NotFound($"Tournament with Id '{tournamentId}' was not found.");

        var game = mapper.Map<Game>(gameDto);
        game.TournamentDetailsId = tournamentId;
        try
        {
            unitOfWork.GameRepository.Add(game);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
        return CreatedAtAction(nameof(GetGames), new { tournamentId, id = game.Id }, mapper.Map<GameDto>(game));
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchGame(int tournamentId, int id, JsonPatchDocument<GameEditDto> patchDoc)
    {
        if (patchDoc == null)
            return BadRequest("Patch document cannot be null.");

        if (!await unitOfWork.TournamentRepository.AnyAsync(tournamentId))
            return NotFound($"Tournament with Id '{tournamentId}' was not found.");

        var game = await unitOfWork.GameRepository.GetByIdAsync(id);
        if (game == null)
            return NotFound($"Game with id '{id}' does not exist.");

        if (tournamentId != game.TournamentDetailsId)
            return BadRequest($"Game with id '{id}' does not belong to tournament with Id '{tournamentId}'.");

        var gameToPatch = mapper.Map<GameEditDto>(game);
        patchDoc.ApplyTo(gameToPatch, ModelState);
        TryValidateModel(gameToPatch);
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);

        mapper.Map(gameToPatch, game);
        try
        {
            if (unitOfWork.HasChanges())
                await unitOfWork.CompleteAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (!await unitOfWork.GameRepository.AnyAsync(id))
                return NotFound($"Game with id '{id}' does not exist.");
            return Conflict(new
            {
                Message = "Concurrency error occurred while updating the game.",
                Details = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGames(int tournamentId, int id)
    {
        if (!await unitOfWork.TournamentRepository.AnyAsync(tournamentId))
            return NotFound($"Tournament with Id '{tournamentId}' was not found.");

        var game = await unitOfWork.GameRepository.GetByIdAsync(id);
        if (game == null)
            return NotFound($"Game with id '{id}' does not exist.");

        if (game.TournamentDetailsId != tournamentId)
            return BadRequest($"Game with id '{id}' does not belong to tournament with Id '{tournamentId}'.");

        try
        {
            unitOfWork.GameRepository.Remove(game);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception ex)
        {
            StatusCode(500, ex.Message);
        }

        return NoContent();
    }

}
