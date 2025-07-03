using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;
using Tournament.Presentation.Extensions;
using Tournament.Shared.DTOs;
using Tournament.Shared.Responses;

namespace Tournament.Presentation.Controllers;
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

        var games = await unitOfWork.GameRepository.SearchGamesByTitleAsync(tournamentId, queryParameters.SearchTerm);

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

    [HttpGet("{title}")]
    public async Task<ActionResult<GameDto>> GetGame(int tournamentId, string title)
    {
        if (!await unitOfWork.TournamentRepository.AnyAsync(tournamentId))
            return NotFound($"Tournament with Id '{tournamentId}' was not found.");

        var games = await unitOfWork.GameRepository.GetGamesByTitleAsync(tournamentId, title);
        if (games == null || !games.Any())
            return NotFound($"Game with Title '{title}' was not found.");

        return Ok(mapper.Map<IEnumerable<GameDto>>(games));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutGame(int tournamentId, int id, GameEditDto game)
    {
        var response = new ApiResponse<GameDto>();

        if (!ModelState.IsValid)
        {
            response.Success = false;
            response.Errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            return this.HandleApiResponse(response);
        }

        try
        {
            if (!await unitOfWork.TournamentRepository.AnyAsync(tournamentId))
            {
                response.Success = false;
                response.Message = $"Tournament with Id '{tournamentId}' was not found.";
                return this.HandleApiResponse(response);
            }

            if (id != game.Id)
            {
                response.Success = false;
                response.Message = "Game ID mismatch.";
                return this.HandleApiResponse(response);
            }

            var existingGame = await unitOfWork.GameRepository.FindByIdAsync(id, true);
            if (existingGame == null)
            {
                response.Success = false;
                response.Message = $"Game with id '{id}' does not exist.";
                return this.HandleApiResponse(response);
            }

            mapper.Map(game, existingGame);

            if (unitOfWork.HasChanges())
                await unitOfWork.CompleteAsync();

            response.Success = true;
            response.Data = mapper.Map<GameDto>(existingGame);
            return this.HandleApiResponse(response);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            response.Success = false;
            response.Message = "Concurrency error occurred while updating the game.";
            response.Errors = [ex.Message];
            return this.HandleApiResponse(response);
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = "An error occurred while processing your request.";
            response.Errors = [ex.Message];
            return this.HandleApiResponse(response);
        }
    }

    [HttpPost]
    public async Task<ActionResult<Game>> PostGames(int tournamentId, GameCreateDto gameDto)
    {
        var response = new ApiResponse<GameDto>();
        if (!ModelState.IsValid)
        {
            response.Status = 422;
            response.Success = false;
            response.Errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            return this.HandleApiResponse(response);
        }

        if (!await unitOfWork.TournamentRepository.AnyAsync(tournamentId))
        {
            response.Success = false;
            response.Status = 404;
            response.Message = $"Tournament with Id '{tournamentId}' was not found.";
            return this.HandleApiResponse(response);
        }

        var game = mapper.Map<Game>(gameDto);
        game.TournamentDetailsId = tournamentId;
        try
        {
            unitOfWork.GameRepository.Add(game);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Status = 500;
            response.Message = "An error occurred while processing your request.";
            response.Errors = [ex.Message];
            return this.HandleApiResponse(response);
        }
        return CreatedAtAction(nameof(GetGames), new { tournamentId, id = game.Id }, mapper.Map<GameDto>(game));
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchGame(int tournamentId, int id, JsonPatchDocument<GameEditDto> patchDoc)
    {
        var response = new ApiResponse<GameEditDto>
        {
            Success = true
        };

        if (patchDoc == null)
        {
            response.Success = false;
            response.Status = 400;
            response.Message = "Patch document cannot be null.";
            return this.HandleApiResponse(response);
        }

        if (!await unitOfWork.TournamentRepository.AnyAsync(tournamentId))
        {
            response.Success = false;
            response.Status = 404;
            response.Message = $"Tournament with Id '{tournamentId}' was not found.";
            return this.HandleApiResponse(response);
        }

        var game = await unitOfWork.GameRepository.FindByIdAsync(id, true);
        if (game == null)
        {
            response.Success = false;
            response.Status = 404;
            response.Message = $"Game with id '{id}' does not exist.";
            return this.HandleApiResponse(response);
        }

        if (tournamentId != game.TournamentDetailsId)
        {
            response.Success = false;
            response.Status = 400;
            response.Message = $"Game with id '{id}' does not belong to tournament with Id '{tournamentId}'.";
            return this.HandleApiResponse(response);
        }

        var gameToPatch = mapper.Map<GameEditDto>(game);
        patchDoc.ApplyTo(gameToPatch, ModelState);
        TryValidateModel(gameToPatch);
        if (!ModelState.IsValid)
        {
            response.Success = false;
            response.Status = 422;
            response.Errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            return this.HandleApiResponse(response);
        }

        mapper.Map(gameToPatch, game);
        try
        {
            if (unitOfWork.HasChanges())
                await unitOfWork.CompleteAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            response.Success = false;
            if (!await unitOfWork.GameRepository.AnyAsync(id))
            {
                response.Status = 404;
                response.Message = $"Game with id '{id}' does not exist.";
            }
            else
            {
                response.Status = 409;
                response.Message = "Concurrency error occurred while updating the game.";
                response.Errors = [ex.Message];
            }
            return this.HandleApiResponse(response);

        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Status = 500;
            response.Message = "An error occurred while processing your request.";
            response.Errors = [ex.Message];
            return this.HandleApiResponse(response);
        }
        return this.HandleApiResponse(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGames(int tournamentId, int id)
    {
        if (!await unitOfWork.TournamentRepository.AnyAsync(tournamentId))
            return NotFound($"Tournament with Id '{tournamentId}' was not found.");

        var game = await unitOfWork.GameRepository.FindByIdAsync(id);
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
