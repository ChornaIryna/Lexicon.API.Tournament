using Microsoft.AspNetCore.JsonPatch;
using Tournament.Shared.DTOs;
using Tournament.Shared.Responses;

namespace Service.Contracts.Interfaces;
public interface ITournamentService
{
    Task<ApiResponse<IEnumerable<TournamentDto>>> GetTournamentsAsync(QueryParameters queryParameters, bool includeGames = false);
    Task<IEnumerable<TournamentDto>> GetAllAsync(bool includeGames = false);
    Task<ApiResponse<TournamentDto?>> GetByIdAsync(int id, bool includeGames = false);
    Task<ApiResponse<TournamentDto>> CreateAsync(TournamentCreateDto tournamentDto);
    Task<ApiResponse<object>> UpdateAsync(int id, TournamentEditDto tournamentDto);
    Task<ApiResponse<object>> DeleteAsync(int id);
    Task<ApiResponse<TournamentDto>> PatchAsync(int id, JsonPatchDocument<TournamentEditDto> patchDoc);
}
