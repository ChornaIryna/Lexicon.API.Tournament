using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts.Interfaces;
using Tournament.Presentation.Extensions;
using Tournament.Shared.DTOs;

namespace Tournament.Presentation.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TournamentsController(IServiceManager serviceManager) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TournamentDto>>> GetTournaments([FromQuery] QueryParameters queryParameters, [FromQuery] bool includeGames = false) =>
        this.HandleApiResponse(await serviceManager.TournamentService.GetAllAsync(queryParameters, includeGames));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TournamentDto>> GetTournamentDetails(int id, [FromQuery] bool includeGames = false) =>
        this.HandleApiResponse(await serviceManager.TournamentService.GetByIdAsync(id, includeGames));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutTournamentDetails(int id, TournamentEditDto tournamentDto) =>
        this.HandleApiResponse(await serviceManager.TournamentService.UpdateAsync(id, tournamentDto));

    [HttpPost]
    public async Task<ActionResult<TournamentDto>> PostTournamentDetails(TournamentCreateDto tournamentDto)
    {
        var response = await serviceManager.TournamentService.CreateAsync(tournamentDto);
        return response.Success
            ? CreatedAtAction(nameof(GetTournaments), response.Data)
            : this.HandleApiResponse(response);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> PatchTournamentDetails(int id, JsonPatchDocument<TournamentEditDto> patchDoc) =>
        this.HandleApiResponse(await serviceManager.TournamentService.UpdateAsync(id, patchDoc));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTournamentDetails(int id) =>
        this.HandleApiResponse(await serviceManager.TournamentService.DeleteAsync(id));
}
