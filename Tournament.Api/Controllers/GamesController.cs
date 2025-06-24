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
    public async Task<ActionResult<IEnumerable<GameDto>>> GetGames(int? tournamentId = null)
    {
        if (!await unitOfWork.GameRepository.AnyAsync())
            return NotFound("No gameDto found");

        if (!tournamentId.HasValue)
            return Ok(mapper.Map<IEnumerable<GameDto>>(await unitOfWork.GameRepository.GetAllGamesAsync()));
        else
        {
            var games = await unitOfWork.GameRepository.GetAllTournamentGamesAsync(tournamentId.Value);
            return (games != null && games.Any()) ?
                Ok(mapper.Map<IEnumerable<GameDto>>(games))
                : NotFound("No gameDto found for the specified tournament.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GameDto>> GetGames(int tournamentId, int id)
    {
        if (!await unitOfWork.TournamentRepository.TournamentExists(tournamentId))
            return NotFound($"Tournament with Id '{tournamentId}' was not found.");
        var game = await unitOfWork.GameRepository.GetGameByIdAsync(id);
        if (game == null)
            return NotFound($"Game with Id '{id}' was not found.");

        return Ok(mapper.Map<GameDto>(game));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutGame(int tournamentId, int id, GameEditDto game)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!await unitOfWork.TournamentRepository.TournamentExists(tournamentId))
            return NotFound($"Tournament with Id '{tournamentId}' was not found.");

        if (id != game.Id)
            return BadRequest("Game ID mismatch.");

        var existingGame = await unitOfWork.GameRepository.GetGameByIdAsync(id);
        if (existingGame == null)
            return NotFound($"Game with id '{id}' does not exist.");

        if (existingGame.TournamentDetailsId != tournamentId)
            return BadRequest($"Game with id '{id}' does not belong to tournament with Id '{tournamentId}'.");

        mapper.Map(game, existingGame);
        try
        {
            unitOfWork.GameRepository.Update(existingGame);
            await unitOfWork.CompleteAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (!await unitOfWork.GameRepository.GameExists(id))
                return NotFound($"Game with id '{id}' does not exist.");
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<Game>> PostGames(int tournamentId, GameCreateDto gameDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!await unitOfWork.TournamentRepository.TournamentExists(tournamentId))
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
            return BadRequest(ex.Message);
        }
        return CreatedAtAction(nameof(GetGames), new { tournamentId, id = game.Id }, mapper.Map<GameDto>(game));
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchGame(int tournamentId, int id, JsonPatchDocument<GameEditDto> patchDoc)
    {
        if (patchDoc == null)
            return BadRequest("Patch document cannot be null.");

        if (!await unitOfWork.TournamentRepository.TournamentExists(tournamentId))
            return NotFound($"Tournament with Id '{tournamentId}' was not found.");

        var game = await unitOfWork.GameRepository.GetGameByIdAsync(id);
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
            unitOfWork.GameRepository.Update(game);
            await unitOfWork.CompleteAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (!await unitOfWork.GameRepository.GameExists(id))
                return NotFound($"Game with id '{id}' does not exist.");
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGames(int tournamentId, int id)
    {
        if (!await unitOfWork.TournamentRepository.TournamentExists(tournamentId))
            return NotFound($"Tournament with Id '{tournamentId}' was not found.");

        var game = await unitOfWork.GameRepository.GetGameByIdAsync(id);
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
            return BadRequest(ex.Message);
        }

        return NoContent();
    }

}
