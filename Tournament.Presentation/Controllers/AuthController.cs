using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts.Interfaces;
using Tournament.Presentation.Extensions;
using Tournament.Shared.DTOs;

namespace Tournament.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IServiceManager serviceManager) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] UserRegistrationDto registrationDto) =>
        this.HandleApiResponse(await serviceManager.AuthService.RegisterAsync(registrationDto));
}
