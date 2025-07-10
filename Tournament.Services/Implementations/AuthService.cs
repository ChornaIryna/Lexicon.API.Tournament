using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Service.Contracts.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tournament.Core.Entities;
using Tournament.Shared.DTOs;
using Tournament.Shared.Responses;

namespace Tournament.Services.Implementations;
public class AuthService(IMapper mapper, UserManager<ApplicationUser> userManager, IConfiguration configuration) : ServiceBase, IAuthService
{
    private ApplicationUser? user;
    public async Task<ApiResponse<string>> IsUserAuthenticatedAsync(UserLoginDto userLoginDto)
    {
        ArgumentNullException.ThrowIfNull(userLoginDto);
        if (userLoginDto.UserName == null || userLoginDto.Password == null)
            return CreateErrorResponse<string>(StatusCodes.Status400BadRequest, "Username and password cannot be null.");

        user = await userManager.FindByNameAsync(userLoginDto.UserName);
        if (user == null)
            return CreateErrorResponse<string>(StatusCodes.Status404NotFound, "User not found.");
        var isAuthenticated = await userManager.CheckPasswordAsync(user, userLoginDto.Password);
        if (isAuthenticated)
        {
            var result = new { Token = await CreateTokenAsync() };
            return CreateSuccessResponse(result.Token, StatusCodes.Status200OK, "User authenticated successfully.");
        }
        return CreateErrorResponse<string>(StatusCodes.Status401Unauthorized, "Invalid username or password.");
    }

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

    private async Task<string> CreateTokenAsync()
    {
        SigningCredentials signingCredentials = GetSigningCredentials();
        IEnumerable<Claim> claims = await GetClaimsAsync();
        JwtSecurityToken tokenOptions = GenerateTokenOptions(signingCredentials, claims);
        return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
    }

    private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
    {
        var jwtSettings = configuration.GetSection("JwTSettings");
        return new JwtSecurityToken(
                                    issuer: jwtSettings["Issuer"],
                                    audience: jwtSettings["Audience"],
                                    signingCredentials: signingCredentials,
                                    claims: claims,
                                    expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["ExpirationMinutes"]))
                                    );
    }

    private async Task<IEnumerable<Claim>> GetClaimsAsync()
    {
        ArgumentNullException.ThrowIfNull(nameof(user));
        var claims = new List<Claim>()
        {
            new (ClaimTypes.Name, user!.UserName!),
            new ("Age", user!.Age.ToString())
        };

        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));
        return claims;
    }

    private SigningCredentials GetSigningCredentials()
    {
        var key = configuration["JwTSettings:Key"];
        ArgumentNullException.ThrowIfNull(key);
        byte[] data = Encoding.UTF8.GetBytes(key);
        var secret = new SymmetricSecurityKey(data);
        return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
    }
}
