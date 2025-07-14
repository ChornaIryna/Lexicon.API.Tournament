using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts.Interfaces;
using Tournament.Core.Entities;
using Tournament.Presentation.Extensions;
using Tournament.Shared.DTOs;

namespace Tournament.Presentation.Controllers;
[ApiController]
[Authorize]
[Route("api/tournaments/{tournamentId:int}/[controller]")]
public class GamesController(IServiceManager serviceManager) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameDto>>> GetGames(int tournamentId, [FromQuery] QueryParameters queryParameters) =>
        this.HandleApiResponse(await serviceManager.GameService.GetAllAsync(tournamentId, queryParameters));

    [HttpGet("{title}")]
    public async Task<ActionResult<GameDto>> GetGame(int tournamentId, string title) =>
        this.HandleApiResponse(await serviceManager.GameService.GetByTitleAsync(tournamentId, title));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GameDto>> GetGame(int tournamentId, int id) =>
        this.HandleApiResponse(await serviceManager.GameService.GetByIdAsync(tournamentId, id));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutGame(int tournamentId, int id, GameEditDto game) =>
        this.HandleApiResponse(await serviceManager.GameService.UpdateAsync(tournamentId, id, game));

    [HttpPost]
    public async Task<ActionResult<Game>> PostGames(int tournamentId, GameCreateDto gameDto)
    {
        var response = await serviceManager.GameService.CreateAsync(tournamentId, gameDto);

        return response.Success
            ? CreatedAtAction(nameof(GetGames), new { tournamentId, id = response.MetaData }, response.Data)
            : this.HandleApiResponse(response);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchGame(int tournamentId, int id, JsonPatchDocument<GameEditDto> patchDoc) =>
        this.HandleApiResponse(await serviceManager.GameService.UpdateAsync(tournamentId, id, patchDoc));

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGames(int tournamentId, int id) =>
        this.HandleApiResponse(await serviceManager.GameService.DeleteAsync(tournamentId, id));
}
