using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Service.Contracts.Interfaces;
using Tournament.Core.Entities;
using Tournament.Shared.DTOs;
using Tournament.Shared.Responses;

namespace Tournament.Services.Implementations;
public class AuthService(IMapper mapper, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : ServiceBase, IAuthService
{
    public async Task<ApiResponse<IdentityResult>> RegisterAsync(UserRegistrationDto registrationDto)
    {
        ArgumentNullException.ThrowIfNull(registrationDto);

        var user = mapper.Map<ApplicationUser>(registrationDto);
        user.UserName = registrationDto.Email;
        var result = await userManager.CreateAsync(user, registrationDto.Password);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(registrationDto.Position)
                && registrationDto.Position.Equals("Admin", StringComparison.CurrentCultureIgnoreCase))
                await userManager.AddToRoleAsync(user, "Admin");
            else
                await userManager.AddToRoleAsync(user, "User");
        }

        return CreateSuccessResponse(result, StatusCodes.Status201Created, "User registered successfully.");

    }
}
