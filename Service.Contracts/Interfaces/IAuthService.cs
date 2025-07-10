using Microsoft.AspNetCore.Identity;
using Tournament.Shared.DTOs;
using Tournament.Shared.Responses;

namespace Service.Contracts.Interfaces;
public interface IAuthService
{
    Task<ApiResponse<IdentityResult>> RegisterAsync(UserRegistrationDto registrationDto);
    Task<ApiResponse<TokenDto>> IsUserAuthenticatedAsync(UserLoginDto userLoginDto);
    Task<ApiResponse<TokenDto>> RefreshTokenAsync(TokenDto tokenDto);
}
