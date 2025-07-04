using Microsoft.AspNetCore.JsonPatch;
using Tournament.Shared.DTOs;
using Tournament.Shared.Responses;

namespace Service.Contracts.Interfaces;
public interface ITournamentService
{
    Task<ApiResponse<IEnumerable<TournamentDto>>> GetAllAsync(QueryParameters queryParameters, bool includeGames = false);
    Task<ApiResponse<TournamentDto>> GetByIdAsync(int id, bool includeGames = false);
    Task<ApiResponse<TournamentDto>> CreateAsync(TournamentCreateDto tournamentDto);
    Task<ApiResponse<object>> UpdateAsync(int id, TournamentEditDto tournamentDto);
    Task<ApiResponse<TournamentDto>> UpdateAsync(int id, JsonPatchDocument<TournamentEditDto> patchDoc);
    Task<ApiResponse<object>> DeleteAsync(int id);
}
