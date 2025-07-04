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
public class TournamentService(IUoW unitOfWork, IMapper mapper) : ServiceBase, ITournamentService
{
    public async Task<ApiResponse<TournamentDto>> CreateAsync(TournamentCreateDto tournamentDto)
    {
        ArgumentNullException.ThrowIfNull(tournamentDto);
        var tournament = mapper.Map<TournamentDetails>(tournamentDto);

        if (!ValidateEntity(tournament, out var errors))
            return CreateErrorResponse<TournamentDto>(
                StatusCodes.Status400BadRequest,
                "Validation failed",
                errors);

        unitOfWork.TournamentRepository.Add(tournament);
        try
        {
            await unitOfWork.CompleteAsync();
            var createdTournamentDto = mapper.Map<TournamentDto>(tournament);
            return CreateSuccessResponse(
                createdTournamentDto,
                StatusCodes.Status201Created,
                "Tournament created successfully");
        }
        catch (Exception ex)
        {
            return CreateErrorResponse<TournamentDto>(
                StatusCodes.Status500InternalServerError,
                "Failed to create tournament",
                [ex.Message]);
        }
    }
    public async Task<ApiResponse<object>> DeleteAsync(int id)
    {
        if (!await unitOfWork.TournamentRepository.AnyAsync(id))
            return CreateErrorResponse<object>(
                StatusCodes.Status400BadRequest,
                "Invalid tournament ID");

        var tournamentDetails = await unitOfWork.TournamentRepository.FindByIdAsync(id, true);
        if (tournamentDetails == null)
            return CreateErrorResponse<object>(
                StatusCodes.Status404NotFound,
                $"Tournament with id '{id}' was not found");

        unitOfWork.TournamentRepository.Remove(tournamentDetails);
        try
        {
            await unitOfWork.CompleteAsync();
            return CreateSuccessResponse<object>(
                null,
                StatusCodes.Status204NoContent,
                "Tournament deleted successfully");
        }
        catch (Exception ex)
        {
            return CreateErrorResponse<object>(
                StatusCodes.Status500InternalServerError,
                "Failed to delete tournament",
                [ex.Message]);
        }
    }

    public async Task<ApiResponse<TournamentDto>> GetByIdAsync(int id, bool includeGames = false)
    {
        var tournamentDetails = await unitOfWork.TournamentRepository.FindByIdAsync(id);

        if (tournamentDetails == null)
            return CreateErrorResponse<TournamentDto>(
                StatusCodes.Status404NotFound,
                $"Tournament with id '{id}' was not found");

        var tournamentDto = includeGames
            ? mapper.Map<TournamentWithGamesDto>(tournamentDetails)
            : mapper.Map<TournamentDto>(tournamentDetails);
        return CreateSuccessResponse(
            tournamentDto,
            StatusCodes.Status200OK,
            "Tournament retrieved successfully");
    }

    public async Task<ApiResponse<IEnumerable<TournamentDto>>> GetAllAsync(QueryParameters queryParameters, bool includeGames = false)
    {
        ArgumentNullException.ThrowIfNull(queryParameters);
        if (!ValidateQueryParameters(queryParameters, out var errorResponse))
            return CreateErrorResponse<IEnumerable<TournamentDto>>(errorResponse!.Status, errorResponse.Message);

        var query = unitOfWork.TournamentRepository
                                    .GetFiltered(t => queryParameters.SearchTerm == null
                                                || t.Title.Contains(queryParameters.SearchTerm),
                                                includeGames);
        if (!query.Any())
            return CreateErrorResponse<IEnumerable<TournamentDto>>(
                StatusCodes.Status404NotFound,
                "No tournaments found");

        query = ApplyOrdering(query, queryParameters);

        var (paginatedTournaments, totalCount) = await unitOfWork.TournamentRepository
                                                    .GetPagedAsync(query, queryParameters.PageNumber, queryParameters.PageSize);


        var tournamentsDto = includeGames
                            ? mapper.Map<IEnumerable<TournamentWithGamesDto>>(paginatedTournaments)
                            : mapper.Map<IEnumerable<TournamentDto>>(paginatedTournaments);
        return CreateSuccessResponse(
            tournamentsDto,
            StatusCodes.Status200OK,
            "Tournaments retrieved successfully",
            CreatePaginationMetadata(totalCount, queryParameters));
    }

    public async Task<ApiResponse<TournamentDto>> UpdateAsync(int id, JsonPatchDocument<TournamentEditDto> patchDoc)
    {
        if (patchDoc == null)
            return CreateErrorResponse<TournamentDto>(
                StatusCodes.Status400BadRequest,
                "Patch document cannot be null");

        var tournamentDetails = await unitOfWork.TournamentRepository.FindByIdAsync(id, true);
        if (tournamentDetails == null)
            return CreateErrorResponse<TournamentDto>(
                StatusCodes.Status404NotFound,
                $"Tournament with id '{id}' was not found");

        var tournamentEditDto = mapper.Map<TournamentEditDto>(tournamentDetails);
        List<string> patchErrors = [];

        patchDoc.ApplyTo(tournamentEditDto, error =>
        {
            patchErrors.Add($"Error applying patch: {error.ErrorMessage}");
        });

        if (patchErrors.Count != 0)
            return CreateErrorResponse<TournamentDto>(
                StatusCodes.Status400BadRequest,
                "Failed to apply patch to tournament",
                patchErrors);

        IEnumerable<string> errors = [];
        if (!ValidateEntity(tournamentEditDto, out errors))
            return CreateErrorResponse<TournamentDto>(
                StatusCodes.Status422UnprocessableEntity,
                "Validation failed",
                errors);

        mapper.Map(tournamentEditDto, tournamentDetails);
        try
        {
            if (unitOfWork.HasChanges())
                await unitOfWork.CompleteAsync();

            var resultDto = mapper.Map<TournamentDto>(tournamentDetails);
            return CreateSuccessResponse(
                resultDto,
                StatusCodes.Status200OK,
                "Tournament updated successfully");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (!await unitOfWork.TournamentRepository.AnyAsync(id))
                return CreateErrorResponse<TournamentDto>(
                    StatusCodes.Status404NotFound,
                    $"Tournament with id '{id}' was not found",
                    [ex.Message]);

            return CreateErrorResponse<TournamentDto>(
                StatusCodes.Status409Conflict,
                "Concurrency conflict occurred while applying patch to tournament",
                [ex.Message]);
        }
        catch (Exception ex)
        {
            return CreateErrorResponse<TournamentDto>(
                StatusCodes.Status500InternalServerError,
                "Failed to apply patch to tournament",
                [ex.Message]);
        }
    }

    public async Task<ApiResponse<object>> UpdateAsync(int id, TournamentEditDto tournamentDto)
    {
        if (id != tournamentDto.Id || !await unitOfWork.TournamentRepository.AnyAsync(id))
            return CreateErrorResponse<object>(
                StatusCodes.Status400BadRequest,
                "Invalid tournament ID");

        var tournamentDetails = await unitOfWork.TournamentRepository.FindByIdAsync(id, true);
        if (tournamentDetails == null)
            return CreateErrorResponse<object>(
                StatusCodes.Status404NotFound,
                $"Tournament with id '{id}' was not found");

        mapper.Map(tournamentDto, tournamentDetails);

        if (!ValidateEntity(tournamentDetails, out var errors))
            return CreateErrorResponse<object>(
                StatusCodes.Status400BadRequest,
                "Validation failed",
                errors);

        try
        {
            if (unitOfWork.HasChanges())
                await unitOfWork.CompleteAsync();
            return CreateSuccessResponse<object>(
                null,
                StatusCodes.Status204NoContent,
                "Tournament updated successfully");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (!await unitOfWork.TournamentRepository.AnyAsync(id))
            {
                return CreateErrorResponse<object>(
                    StatusCodes.Status404NotFound,
                    $"Tournament with id '{id}' was not found",
                    [ex.Message]);
            }
            return CreateErrorResponse<object>(
                StatusCodes.Status409Conflict,
                "Concurrency conflict occurred while updating the tournament",
                [ex.Message]);
        }
        catch (Exception ex)
        {
            return CreateErrorResponse<object>(
                StatusCodes.Status500InternalServerError,
                "Failed to update tournament",
                [ex.Message]);
        }
    }

    private static IQueryable<TournamentDetails> ApplyOrdering(IQueryable<TournamentDetails> tournaments, QueryParameters queryParameters)
    {
        if (string.IsNullOrEmpty(queryParameters.OrderBy)) return tournaments;

        return queryParameters.OrderBy.ToLower() switch
        {
            "title" => tournaments.OrderBy(t => t.Title),
            "startdate" => tournaments.OrderBy(t => t.StartDate),
            _ => tournaments
        };
    }
}
