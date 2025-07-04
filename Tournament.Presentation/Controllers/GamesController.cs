using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts.Interfaces;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;
using Tournament.Presentation.Extensions;
using Tournament.Shared.DTOs;

namespace Tournament.Presentation.Controllers;
[AllowAnonymous]
[Route("api/tournaments/{tournamentId:int}/[controller]")]
[ApiController]
public class GamesController(IMapper mapper, IUoW unitOfWork, IServiceManager serviceManager) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameDto>>> GetGames(int tournamentId, [FromQuery] QueryParameters queryParameters)
    {
        var response = await serviceManager.GameService.GetAllAsync(tournamentId, queryParameters);
        return this.HandleApiResponse(response);
    }

    [HttpGet("{title}")]
    public async Task<ActionResult<GameDto>> GetGame(int tournamentId, string title)
    {
        var response = await serviceManager.GameService.GetByTitleAsync(tournamentId, title);
        return this.HandleApiResponse(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GameDto>> GetGame(int tournamentId, int id)
    {
        var response = await serviceManager.GameService.GetByIdAsync(tournamentId, id);
        return this.HandleApiResponse(response);

    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutGame(int tournamentId, int id, GameEditDto game)
    {
        var response = await serviceManager.GameService.UpdateAsync(tournamentId, id, game);
        return this.HandleApiResponse(response);
    }

    [HttpPost]
    public async Task<ActionResult<Game>> PostGames(int tournamentId, GameCreateDto gameDto)
    {
        var response = await serviceManager.GameService.CreateAsync(tournamentId, gameDto);

        return response.Success
            ? CreatedAtAction(nameof(GetGames), new { tournamentId, id = response.MetaData }, mapper.Map<GameDto>(response.Data))
            : this.HandleApiResponse(response);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchGame(int tournamentId, int id, JsonPatchDocument<GameEditDto> patchDoc)
    {
        var response = await serviceManager.GameService.UpdateAsync(tournamentId, id, patchDoc);
        return this.HandleApiResponse(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGames(int tournamentId, int id)
    {
        var response = await serviceManager.GameService.DeleteAsync(tournamentId, id);
        return this.HandleApiResponse(response);
    }

}
