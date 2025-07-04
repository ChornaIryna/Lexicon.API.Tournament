using Microsoft.AspNetCore.JsonPatch;
using Tournament.Shared.DTOs;
using Tournament.Shared.Responses;

namespace Service.Contracts.Interfaces;
public interface IGameService
{
    Task<ApiResponse<IEnumerable<GameDto>>> GetAllAsync(int tournamentId, QueryParameters queryParameters);
    Task<ApiResponse<GameDto>> GetByIdAsync(int tournamentId, int id);
    Task<ApiResponse<GameDto>> GetByTitleAsync(int tournamentId, string title);
    Task<ApiResponse<GameDto>> CreateAsync(int tournamentId, GameCreateDto tournamentDto);
    Task<ApiResponse<object>> UpdateAsync(int tournamentId, int id, GameEditDto tournamentDto);
    Task<ApiResponse<GameDto>> UpdateAsync(int tournamentId, int id, JsonPatchDocument<GameEditDto> patchDoc);
    Task<ApiResponse<object>> DeleteAsync(int tournamentId, int id);
}
