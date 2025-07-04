using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Service.Contracts.Interfaces;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;
using Tournament.Shared.DTOs;
using Tournament.Shared.Responses;

namespace Tournament.Services.Implementations;
public class GameService(IUoW unitOfWork, IMapper mapper) : ServiceBase, IGameService
{
    public async Task<ApiResponse<IEnumerable<GameDto>>> GetAllAsync(int tournamentId, QueryParameters queryParameters)
    {
        ArgumentNullException.ThrowIfNull(queryParameters, nameof(queryParameters));
        if (!ValidateQueryParameters(queryParameters, out var errorResponse))
            return CreateErrorResponse<IEnumerable<GameDto>>(errorResponse!.Status, errorResponse.Message);

        if (!await unitOfWork.TournamentRepository.AnyAsync(tournamentId))
            return CreateErrorResponse<IEnumerable<GameDto>>(StatusCodes.Status404NotFound, $"Tournament with Id '{tournamentId}' was not found.");

        var games = unitOfWork.GameRepository.GetFiltered(g => g.TournamentDetailsId == tournamentId
                                                                    && (queryParameters.SearchTerm == null
                                                                    || g.Title.Contains(queryParameters.SearchTerm)));

        if (games == null || !games.Any())
            return CreateErrorResponse<IEnumerable<GameDto>>(StatusCodes.Status404NotFound, $"No games found for tournament with Id '{tournamentId}'.");

        games = ApplyOrdering(games, queryParameters);

        var (paginatedGames, totalCount) = await unitOfWork.GameRepository.GetPagedAsync(games, queryParameters.PageNumber, queryParameters.PageSize);

        var gamesDto = mapper.Map<IEnumerable<GameDto>>(paginatedGames);
        return CreateSuccessResponse(gamesDto, StatusCodes.Status200OK, "Games retrieved successfully",
            CreatePaginationMetadata(totalCount, queryParameters));
    }
    public async Task<ApiResponse<GameDto>> GetByIdAsync(int tournamentId, int id)
    {
        if (!await unitOfWork.TournamentRepository.AnyAsync(tournamentId))
            return CreateErrorResponse<GameDto>(StatusCodes.Status404NotFound, $"Tournament with Id '{tournamentId}' was not found.");

        var game = await unitOfWork.GameRepository.GetByCondition(g => g.TournamentDetailsId == tournamentId && g.Id == id).FirstOrDefaultAsync();
        if (game == null)
            return CreateErrorResponse<GameDto>(StatusCodes.Status404NotFound, $"Game with Id '{id}' was not found.");
        var gameDto = mapper.Map<GameDto>(game);
        return CreateSuccessResponse(gameDto, StatusCodes.Status200OK, "Game retrieved successfully");
    }

    public async Task<ApiResponse<GameDto>> GetByTitleAsync(int tournamentId, string title)
    {
        if (!await unitOfWork.TournamentRepository.AnyAsync(tournamentId))
            return CreateErrorResponse<GameDto>(StatusCodes.Status404NotFound, $"Tournament with Id '{tournamentId}' was not found.");

        var game = await unitOfWork.GameRepository.GetByCondition(g => g.TournamentDetailsId == tournamentId && g.Title.Equals(title)).FirstOrDefaultAsync();
        if (game == null)
            return CreateErrorResponse<GameDto>(StatusCodes.Status404NotFound, $"Game with Title '{title}' was not found.");
        var gamesDto = mapper.Map<GameDto>(game);
        return CreateSuccessResponse(gamesDto, StatusCodes.Status200OK, "Game retrieved successfully");
    }

    public async Task<ApiResponse<GameDto>> CreateAsync(int tournamentId, GameCreateDto gameDto)
    {
        if (!ValidateEntity(gameDto, out var errors))
            return CreateErrorResponse<GameDto>(StatusCodes.Status400BadRequest, "Invalid game data", errors);


        if (!await unitOfWork.TournamentRepository.AnyAsync(tournamentId))
            return CreateErrorResponse<GameDto>(StatusCodes.Status404NotFound, $"Tournament with Id '{tournamentId}' was not found.");

        var game = mapper.Map<Game>(gameDto);
        game.TournamentDetailsId = tournamentId;
        try
        {
            unitOfWork.GameRepository.Add(game);
            await unitOfWork.CompleteAsync();
            return CreateSuccessResponse(mapper.Map<GameDto>(game), StatusCodes.Status201Created, "Game created successfully", new { game.Id });
        }
        catch (Exception ex)
        {
            return CreateErrorResponse<GameDto>(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.", [ex.Message]);
        }
    }
    public async Task<ApiResponse<object>> UpdateAsync(int tournamentId, int id, GameEditDto gameEditDto)
    {
        if (!ValidateEntity(gameEditDto, out var errors))
            return CreateErrorResponse<object>(StatusCodes.Status400BadRequest, "Invalid game data", errors);
        try
        {
            if (!await unitOfWork.TournamentRepository.AnyAsync(tournamentId))
                return CreateErrorResponse<object>(StatusCodes.Status404NotFound, $"Tournament with Id '{tournamentId}' was not found.");


            if (id != gameEditDto.Id)
                return CreateErrorResponse<object>(StatusCodes.Status400BadRequest, "Game ID mismatch.");

            var existingGame = await unitOfWork.GameRepository.FindByIdAsync(id, true);
            if (existingGame == null)
                return CreateErrorResponse<object>(StatusCodes.Status404NotFound, $"Game with id '{id}' does not exist.");

            mapper.Map(gameEditDto, existingGame);

            if (unitOfWork.HasChanges())
                await unitOfWork.CompleteAsync();
            return CreateSuccessResponse<object>(mapper.Map<GameDto>(existingGame), StatusCodes.Status204NoContent, "Game updated successfully");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return CreateErrorResponse<object>(StatusCodes.Status409Conflict, "Concurrency error occurred while updating the game.", [ex.Message]);
        }
        catch (Exception ex)
        {
            return CreateErrorResponse<object>(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.", [ex.Message]);
        }
    }
    public async Task<ApiResponse<GameDto>> UpdateAsync(int tournamentId, int id, JsonPatchDocument<GameEditDto> patchDoc)
    {
        if (patchDoc == null)
            return CreateErrorResponse<GameDto>(StatusCodes.Status400BadRequest, "Patch document cannot be null.");

        if (!await unitOfWork.TournamentRepository.AnyAsync(tournamentId))
            return CreateErrorResponse<GameDto>(StatusCodes.Status404NotFound, $"Tournament with Id '{tournamentId}' was not found.");

        var game = await unitOfWork.GameRepository.FindByIdAsync(id, true);
        if (game == null)
            return CreateErrorResponse<GameDto>(StatusCodes.Status404NotFound, $"Game with id '{id}' does not exist.");

        if (tournamentId != game.TournamentDetailsId)
            return CreateErrorResponse<GameDto>(StatusCodes.Status400BadRequest, $"Game with id '{id}' does not belong to tournament with Id '{tournamentId}'.");

        var gameToPatch = mapper.Map<GameEditDto>(game);
        List<string> patchErrors = [];
        patchDoc.ApplyTo(gameToPatch, error =>
        {
            patchErrors.Add($"Error applying patch: {error.ErrorMessage}");
        });
        if (patchErrors.Count > 0)
            return CreateErrorResponse<GameDto>(StatusCodes.Status400BadRequest, "Errors occurred while applying the patch.", patchErrors);

        if (!ValidateEntity(gameToPatch, out var validationErrors))
            return CreateErrorResponse<GameDto>(StatusCodes.Status400BadRequest, "Invalid game data", validationErrors);

        mapper.Map(gameToPatch, game);
        try
        {
            if (unitOfWork.HasChanges())
                await unitOfWork.CompleteAsync();
            var updatedGameDto = mapper.Map<GameDto>(game);
            return CreateSuccessResponse(updatedGameDto, StatusCodes.Status200OK, "Game updated successfully");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (!await unitOfWork.GameRepository.AnyAsync(id))
                return CreateErrorResponse<GameDto>(StatusCodes.Status404NotFound, $"Game with id '{id}' does not exist.");
            else
                return CreateErrorResponse<GameDto>(StatusCodes.Status409Conflict, "Concurrency error occurred while updating the game.", [ex.Message]);
        }
        catch (Exception ex)
        {
            return CreateErrorResponse<GameDto>(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.", [ex.Message]);
        }
    }
    public async Task<ApiResponse<object>> DeleteAsync(int tournamentId, int id)
    {
        if (!await unitOfWork.TournamentRepository.AnyAsync(tournamentId))
            return CreateErrorResponse<object>(StatusCodes.Status404NotFound, $"Tournament with Id '{tournamentId}' was not found.");

        var game = await unitOfWork.GameRepository.FindByIdAsync(id);
        if (game == null)
            return CreateErrorResponse<object>(StatusCodes.Status404NotFound, $"Game with id '{id}' does not exist.");

        if (game.TournamentDetailsId != tournamentId)
            return CreateErrorResponse<object>(StatusCodes.Status400BadRequest, $"Game with id '{id}' does not belong to tournament with Id '{tournamentId}'.");

        try
        {
            unitOfWork.GameRepository.Remove(game);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception ex)
        {
            return CreateErrorResponse<object>(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.", [ex.Message]);
        }
        return CreateSuccessResponse<object>(null, StatusCodes.Status204NoContent, "Game deleted successfully");
    }

    private static IQueryable<Game> ApplyOrdering(IQueryable<Game> games, QueryParameters queryParameters)
    {
        if (string.IsNullOrEmpty(queryParameters.OrderBy)) return games;

        return queryParameters.OrderBy.ToLower() switch
        {
            "title" => games.OrderBy(g => g.Title ?? string.Empty),
            "time" => games.OrderBy(g => g.Time),
            _ => games
        };
    }
}
