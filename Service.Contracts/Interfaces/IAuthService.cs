using Microsoft.AspNetCore.Identity;
using Tournament.Shared.DTOs;
using Tournament.Shared.Responses;

namespace Service.Contracts.Interfaces;
public interface IAuthService
{
    Task<ApiResponse<IdentityResult>> RegisterAsync(UserRegistrationDto registrationDto);
    //Task<string> LoginAsync(UserLoginDto loginDto);
    //Task<string> RefreshTokenAsync(string token);
    //Task LogoutAsync(string token);
    //Task<bool> ValidateTokenAsync(string token);
    //Task<string> GetUserIdFromTokenAsync(string token);
    //Task<string> GetUserNameFromTokenAsync(string token);
    //Task<string> GetUserEmailFromTokenAsync(string token);
    //Task<bool> IsUserInRoleAsync(string token, string role);
    //Task<bool> IsUserAuthenticatedAsync(string token);
    //Task<string> GetUserRoleAsync(string token);
    //Task<string> GetUserPositionAsync(string token);
    //Task<string> GetUserAgeAsync(string token);
    //Task<string> GetUserNameByIdAsync(string userId);
    //Task<string> GetUserEmailByIdAsync(string userId);
    //Task<string> GetUserPositionByIdAsync(string userId);
    //Task<string> GetUserAgeByIdAsync(string userId);
    //Task<bool> IsUserInRoleByIdAsync(string userId, string role);

    Task<ApiResponse<string>> IsUserAuthenticatedAsync(UserLoginDto userLoginDto);
}
