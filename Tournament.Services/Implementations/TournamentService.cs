using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Service.Contracts.Interfaces;
using System.ComponentModel.DataAnnotations;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;
using Tournament.Shared.DTOs;
using Tournament.Shared.Responses;

namespace Tournament.Services.Implementations;
public class TournamentService(IUoW unitOfWork, IMapper mapper) : ITournamentService
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
            return new ApiResponse<TournamentDto>
            {
                Success = true,
                Data = createdTournamentDto,
                Status = StatusCodes.Status201Created,
                Message = "Tournament created successfully"
            };
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
            return new ApiResponse<object>
            {
                Success = true,
                Status = StatusCodes.Status204NoContent,
                Message = "Tournament deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return CreateErrorResponse<object>(
                StatusCodes.Status500InternalServerError,
                "Failed to delete tournament",
                [ex.Message]);
        }
    }

    public async Task<ApiResponse<TournamentDto?>> GetByIdAsync(int id, bool includeGames = false)
    {
        var tournamentDetails = await unitOfWork.TournamentRepository.FindByIdAsync(id);

        if (tournamentDetails == null)
            return CreateErrorResponse<TournamentDto?>(
                StatusCodes.Status404NotFound,
                $"Tournament with id '{id}' was not found");

        var tournamentDto = includeGames
            ? mapper.Map<TournamentWithGamesDto>(tournamentDetails)
            : mapper.Map<TournamentDto>(tournamentDetails);
        return new ApiResponse<TournamentDto?>
        {
            Success = true,
            Status = StatusCodes.Status200OK,
            Message = "Tournament retrieved successfully",
            Data = tournamentDto
        };
    }

    public async Task<ApiResponse<IEnumerable<TournamentDto>>> GetTournamentsAsync(QueryParameters queryParameters, bool includeGames = false)
    {
        ArgumentNullException.ThrowIfNull(queryParameters);

        if (queryParameters.PageNumber < 1 || queryParameters.PageSize < 1)
            return CreateErrorResponse<IEnumerable<TournamentDto>>(
                StatusCodes.Status400BadRequest,
                "Page number and page size must be greater than 0");

        if (queryParameters.PageSize > 100)
            return CreateErrorResponse<IEnumerable<TournamentDto>>(
                StatusCodes.Status400BadRequest,
                "Page size cannot exceed 100");

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
        return new ApiResponse<IEnumerable<TournamentDto>>
        {
            Success = true,
            Status = StatusCodes.Status200OK,
            Message = "Tournaments retrieved successfully",
            Data = tournamentsDto,
            MetaData = new
            {
                TotalCount = totalCount,
                CurrentPage = queryParameters.PageNumber,
                NumberOfEntitiesOnPage = queryParameters.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)queryParameters.PageSize)
            }
        };
    }

    public async Task<ApiResponse<TournamentDto>> PatchAsync(int id, JsonPatchDocument<TournamentEditDto> patchDoc)
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
            return new ApiResponse<TournamentDto>
            {
                Success = true,
                Status = StatusCodes.Status200OK,
                Message = "Tournament patched successfully",
                Data = resultDto
            };
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

        IEnumerable<string> errors = [];
        if (!ValidateEntity(tournamentDetails, out errors))
            return CreateErrorResponse<object>(
                StatusCodes.Status400BadRequest,
                "Validation failed",
                errors);

        try
        {
            if (unitOfWork.HasChanges())
                await unitOfWork.CompleteAsync();
            return new ApiResponse<object>
            {
                Success = true,
                Status = StatusCodes.Status204NoContent,
                Message = "Tournament updated successfully"
            };
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

    private ApiResponse<T> CreateErrorResponse<T>(int status, string message, IEnumerable<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Status = status,
            Message = message,
            Errors = errors ?? Enumerable.Empty<string>()
        };
    }

    private bool ValidateEntity<T>(T entity, out IEnumerable<string> errors)
    {
        var validationContext = new ValidationContext(entity);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(entity, validationContext, validationResults, true);
        errors = validationResults.Select(x => x.ErrorMessage ?? string.Empty);
        return isValid;
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
