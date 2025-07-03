using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts.Interfaces;
using Tournament.Presentation.Extensions;
using Tournament.Shared.DTOs;

namespace Tournament.Presentation.Controllers;

[AllowAnonymous] // Allow anonymous access to the API for demonstration purposes; adjust as needed for security.
[Route("api/[controller]")]
[ApiController]
public class TournamentsController(IServiceManager serviceManager) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TournamentDto>>> GetTournaments([FromQuery] QueryParameters queryParameters, [FromQuery] bool includeGames = false)
    {
        var response = await serviceManager.TournamentService.GetTournamentsAsync(queryParameters, includeGames);
        return this.HandleApiResponse(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TournamentDto>> GetTournamentDetails(int id, [FromQuery] bool includeGames = false)
    {
        var response = await serviceManager.TournamentService.GetByIdAsync(id, includeGames);
        return this.HandleApiResponse(response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutTournamentDetails(int id, TournamentEditDto tournamentDto)
    {
        var response = await serviceManager.TournamentService.UpdateAsync(id, tournamentDto);
        return this.HandleApiResponse(response);
    }

    [HttpPost]
    public async Task<ActionResult<TournamentDto>> PostTournamentDetails(TournamentCreateDto tournamentDto)
    {
        var response = await serviceManager.TournamentService.CreateAsync(tournamentDto);
        return response.Success
            ? CreatedAtAction(nameof(GetTournaments), response.Data)
            : this.HandleApiResponse(response);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> PatchTournamentDetails(int id, JsonPatchDocument<TournamentEditDto> patchDoc)
    {
        var response = await serviceManager.TournamentService.PatchAsync(id, patchDoc);
        return this.HandleApiResponse(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTournamentDetails(int id)
    {
        var response = await serviceManager.TournamentService.DeleteAsync(id);
        return this.HandleApiResponse(response);
    }
}
