using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Service.Contracts.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Tournament.Core.Entities;
using Tournament.Shared.DTOs;
using Tournament.Shared.Responses;

namespace Tournament.Services.Implementations;
public class AuthService(IMapper mapper, UserManager<ApplicationUser> userManager, IConfiguration configuration) : ServiceBase, IAuthService
{
    private ApplicationUser? user;
    public async Task<ApiResponse<TokenDto>> IsUserAuthenticatedAsync(UserLoginDto userLoginDto)
    {
        ArgumentNullException.ThrowIfNull(nameof(userLoginDto));
        if (userLoginDto.UserName == null || userLoginDto.Password == null)
            return CreateErrorResponse<TokenDto>(StatusCodes.Status400BadRequest, "Username and password cannot be null.");

        user = await userManager.FindByNameAsync(userLoginDto.UserName);
        if (user == null)
            return CreateErrorResponse<TokenDto>(StatusCodes.Status404NotFound, "User not found.");
        var isAuthenticated = await userManager.CheckPasswordAsync(user, userLoginDto.Password);
        if (isAuthenticated)
        {
            try
            {
                var result = await CreateTokenAsync(expireTime: true);
                return CreateSuccessResponse(result, StatusCodes.Status200OK, "User authenticated successfully.");
            }
            catch (ArgumentNullException ex)
            {
                return CreateErrorResponse<TokenDto>(StatusCodes.Status404NotFound, ex.Message);
            }
        }
        return CreateErrorResponse<TokenDto>(StatusCodes.Status401Unauthorized, "Invalid username or password.");
    }

    public async Task<ApiResponse<TokenDto>> RefreshTokenAsync(TokenDto tokenDto)
    {
        try
        {
            ClaimsPrincipal claimsPrincipal = GetPrincipalFromExpiredToken(tokenDto.AccessToken);
            ApplicationUser? applicationUser = await userManager.FindByNameAsync(claimsPrincipal.Identity?.Name!);
            if (applicationUser == null
                || applicationUser.RefreshToken != tokenDto.RefreshToken
                || applicationUser.RefreshTokenExpireTime <= DateTime.Now)
                return CreateErrorResponse<TokenDto>(StatusCodes.Status400BadRequest, "Invalid token values provided");

            user = applicationUser;
            var result = await CreateTokenAsync(expireTime: false);
            return CreateSuccessResponse(result, StatusCodes.Status200OK, "Token refreshed successfully");
        }
        catch (SecurityTokenException ex)
        {
            return CreateErrorResponse<TokenDto>(StatusCodes.Status400BadRequest, "Error occurred when trying update token", [$"message: {ex.Message}, error type: {ex.GetType()}"]);
        }
        catch (Exception ex)
        {
            return CreateErrorResponse<TokenDto>(StatusCodes.Status400BadRequest, "Error occurred when trying update token", [ex.Message]);
        }
    }

    public async Task<ApiResponse<IdentityResult>> RegisterAsync(UserRegistrationDto registrationDto)
    {
        ArgumentNullException.ThrowIfNull(nameof(registrationDto));

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

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken)
    {
        var jwtSettings = configuration.GetSection("JwTSettings");
        ArgumentNullException.ThrowIfNull(nameof(jwtSettings));
        var key = jwtSettings["Key"];
        ArgumentNullException.ThrowIfNull(nameof(key));

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        ClaimsPrincipal principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);
        if (securityToken is not JwtSecurityToken token
            || !token.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");
        return principal;
    }

    private async Task<TokenDto> CreateTokenAsync(bool expireTime)
    {
        ArgumentNullException.ThrowIfNull(nameof(user));

        SigningCredentials signingCredentials = GetSigningCredentials();
        IEnumerable<Claim> claims = await GetClaimsAsync();
        JwtSecurityToken tokenOptions = GenerateTokenOptions(signingCredentials, claims);

        user!.RefreshToken = GenerateRefreshToken();

        if (expireTime)
            user.RefreshTokenExpireTime = DateTime.Now.AddDays(7);
        await userManager.UpdateAsync(user);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        return new TokenDto(accessToken, user.RefreshToken!);
    }

    private string? GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
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
            new ("Age", user!.Age.ToString()),
            new (ClaimTypes.NameIdentifier, user.Id)
        };

        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));
        return claims;
    }

    private SigningCredentials GetSigningCredentials()
    {
        var key = configuration["JwTSettings:Key"];
        ArgumentNullException.ThrowIfNull(nameof(key));
        byte[] data = Encoding.UTF8.GetBytes(key!);
        var secret = new SymmetricSecurityKey(data);
        return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
    }
}
