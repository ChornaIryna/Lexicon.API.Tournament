using Microsoft.AspNetCore.Mvc;
using Service.Contracts.Interfaces;
using Tournament.Presentation.Extensions;
using Tournament.Shared.DTOs;

namespace Tournament.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TokenController(IServiceManager serviceManager) : ControllerBase
{
    [HttpPost("refresh")]
    public async Task<ActionResult> RefreshToken(TokenDto tokenDto) =>
        this.HandleApiResponse(await serviceManager.AuthService.RefreshTokenAsync(tokenDto));
}
