using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Tournament.Shared.DTOs;
using Tournament.Shared.Responses;

namespace Tournament.Tests.IntegrationTests;

public class TournamentsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public TournamentsControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetTournamentDetails_WhenUserNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var tournamentId = 1;

        // Act
        var response = await _client.GetAsync($"/api/Tournaments/{tournamentId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetTournamentDetails_WhenUserAuthenticated_ReturnsOk()
    {
        // Arrange
        var tournamentId = 1;
        var token = _factory.GenerateJwtToken("testuser_id", "testuser", "User");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/api/Tournaments/{tournamentId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var tournamentDto = JsonSerializer.Deserialize<TournamentDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(tournamentDto);
    }

    [Fact]
    public async Task GetTournamentDetails_WhenUserAuthenticatedButTournamentNotFound_ReturnsNotFound()
    {
        // Arrange
        var nonExistentTournamentId = 999;
        var token = _factory.GenerateJwtToken("testuser_id", "test@test.user", "User");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/api/Tournaments/{nonExistentTournamentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ApiError>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(errorResponse);
        Assert.Equal(StatusCodes.Status404NotFound, errorResponse.Status);
    }

    [Fact]
    public async Task GetTournamentDetails_WhenUserAuthorizedWithSpecificRole_ReturnsOk()
    {
        // Arrange
        var tournamentId = 1;
        var token = _factory.GenerateJwtToken("admin_id", "adminuser", "Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync($"/api/Tournaments/{tournamentId}");

        // Assert
        response.EnsureSuccessStatusCode(); // This test would only pass if [Authorize(Roles = "Admin")] was on the action/controller
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PutAdminRole_WhenUserIsNotAdmin_ReturnsForbidden()
    {
        // Arrange
        var userRolesEditDto = new UserRolesEditDto("test@test.user", true);
        var token = _factory.GenerateJwtToken("regular_user_id", "regularuser"); // No "Admin" role
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PutAsJsonAsync("/api/Auth/manageAdmin", userRolesEditDto);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PutAdminRole_WhenUserIsAdmin_ReturnsNoContent()
    {
        // Arrange
        var userRolesEditDto = new UserRolesEditDto("user@test.email", true);
        var token = _factory.GenerateJwtToken("admin_id", "adminuser", "Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PutAsJsonAsync("/api/Auth/manageAdmin", userRolesEditDto);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}