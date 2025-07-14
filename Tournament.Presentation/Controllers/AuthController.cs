using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts.Interfaces;
using Tournament.Presentation.Extensions;
using Tournament.Shared.DTOs;

namespace Tournament.Presentation.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class AuthController(IServiceManager serviceManager) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] UserRegistrationDto registrationDto) =>
        this.HandleApiResponse(await serviceManager.AuthService.RegisterAsync(registrationDto));

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] UserLoginDto loginDto) =>
        this.HandleApiResponse(await serviceManager.AuthService.IsUserAuthenticatedAsync(loginDto));

    [HttpPut("manageAdmin")]
    public async Task<IActionResult> PutAdminRole(UserRolesEditDto userEditDto) =>
        this.HandleApiResponse(await serviceManager.AuthService.EditAdminRole(userEditDto));
}
