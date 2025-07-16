using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tournament.Core.Entities;
using Tournament.Presentation.Controllers;
using Tournament.Shared.DTOs;
using Tournament.Shared.Responses;
using Tournament.Tests.Data;
using Tournament.Tests.TestFixtures;

namespace Tournament.Tests.ControllersTests;

public class GamesControllerTests : IClassFixture<GamesControllerFixture>
{
    private readonly GamesControllerFixture fixture;

    public GamesControllerTests(GamesControllerFixture fixture)
    {
        this.fixture = fixture;
        fixture.Clear();
    }

    [Fact]
    public async Task GetGames_WhenGamesExist_ReturnsOkWithGames()
    {
        // Arrange
        var tournamentId = 1;
        var queryParameters = TournamentData.GetQueryParameters();
        var games = TournamentData.GetGameDtos(tournamentId, 3);
        var successResponse = new ApiResponse<IEnumerable<GameDto>>
        {
            Success = true,
            Status = StatusCodes.Status200OK,
            Data = games,
            Message = "Games retrieved successfully",
            MetaData = new { TotalCount = 3, CurrentPage = 1, NumberOfEntitiesOnPage = 10, TotalPages = 1 }
        };

        fixture.MockGameService
            .Setup(s => s.GetAllAsync(tournamentId, queryParameters))
            .ReturnsAsync(successResponse);

        // Act
        var result = await fixture.Controller.GetGames(tournamentId, queryParameters);

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<GameDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<GameDto>>(okResult.Value);
        Assert.Equal(3, Enumerable.Count(returnValue));
        fixture.MockGameService.Verify(s => s.GetAllAsync(tournamentId, queryParameters), Times.Once);
    }

    [Fact]
    public async Task GetGames_WhenNoGamesFound_ReturnsNotFound()
    {
        // Arrange
        var tournamentId = 1;
        var queryParameters = TournamentData.GetQueryParameters();
        var errorResponse = new ApiResponse<IEnumerable<GameDto>>
        {
            Success = false,
            Status = StatusCodes.Status404NotFound,
            Message = "No games found"
        };

        fixture.MockGameService
            .Setup(s => s.GetAllAsync(tournamentId, queryParameters))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await fixture.Controller.GetGames(tournamentId, queryParameters);

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<GameDto>>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
        fixture.MockGameService.Verify(s => s.GetAllAsync(tournamentId, queryParameters), Times.Once);
    }

    [Fact]
    public async Task GetGameById_WithValidGameId_ReturnsOkResultWithGameDto()
    {
        // Arrange
        var tournamentId = 1;
        var gameId = 1;
        var gameDto = TournamentData.GetGameDto(tournamentId, gameId);
        var successResponse = new ApiResponse<GameDto>
        {
            Success = true,
            Status = StatusCodes.Status200OK,
            Data = gameDto,
            Message = "Game retrieved successfully"
        };

        fixture.MockGameService
            .Setup(s => s.GetByIdAsync(tournamentId, gameId))
            .ReturnsAsync(successResponse);

        // Act
        var result = await fixture.Controller.GetGame(tournamentId, gameId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<GameDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<GameDto>(okResult.Value);
        Assert.Equal(gameDto.Title, returnValue.Title);
        fixture.MockGameService.Verify(s => s.GetByIdAsync(tournamentId, gameId), Times.Once);
    }

    [Fact]
    public async Task GetGameById_WhenGameDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var tournamentId = 1;
        var gameId = 999;
        var errorResponse = new ApiResponse<GameDto>
        {
            Success = false,
            Status = StatusCodes.Status404NotFound,
            Message = $"Game with id '{gameId}' was not found"
        };

        fixture.MockGameService
            .Setup(s => s.GetByIdAsync(tournamentId, gameId))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await fixture.Controller.GetGame(tournamentId, gameId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<GameDto>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
        fixture.MockGameService.Verify(s => s.GetByIdAsync(tournamentId, gameId), Times.Once);
    }

    [Fact]
    public async Task GetGameByTitle_WithValidTitle_ReturnsOkResultWithGameDto()
    {
        // Arrange
        var tournamentId = 1;
        var gameId = 1;
        var gameTitle = $"Single Game {gameId} for Tournament {tournamentId}";
        var gameDto = TournamentData.GetGameDto(tournamentId, gameId);
        var successResponse = new ApiResponse<GameDto>
        {
            Success = true,
            Status = StatusCodes.Status200OK,
            Data = gameDto,
            Message = "Game retrieved successfully"
        };

        fixture.MockGameService
            .Setup(s => s.GetByTitleAsync(tournamentId, gameTitle))
            .ReturnsAsync(successResponse);

        // Act
        var result = await fixture.Controller.GetGame(tournamentId, gameTitle);

        // Assert
        var actionResult = Assert.IsType<ActionResult<GameDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<GameDto>(okResult.Value);
        Assert.Equal(gameTitle, returnValue.Title);
        fixture.MockGameService.Verify(s => s.GetByTitleAsync(tournamentId, gameTitle), Times.Once);
    }

    [Fact]
    public async Task GetGameByTitle_WhenGameDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var tournamentId = 1;
        var gameTitle = "NonExistent Game";
        var errorResponse = new ApiResponse<GameDto>
        {
            Success = false,
            Status = StatusCodes.Status404NotFound,
            Message = $"Game with title '{gameTitle}' was not found"
        };

        fixture.MockGameService
            .Setup(s => s.GetByTitleAsync(tournamentId, gameTitle))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await fixture.Controller.GetGame(tournamentId, gameTitle);

        // Assert
        var actionResult = Assert.IsType<ActionResult<GameDto>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
        fixture.MockGameService.Verify(s => s.GetByTitleAsync(tournamentId, gameTitle), Times.Once);
    }

    [Fact]
    public async Task PutGame_OnSuccessfulUpdate_ReturnsNoContent()
    {
        // Arrange
        var tournamentId = 1;
        var gameId = 1;
        var gameEditDto = TournamentData.GetGameEditDto(tournamentId, gameId);
        var successResponse = new ApiResponse<object>
        {
            Success = true,
            Status = StatusCodes.Status204NoContent,
            Message = "Game updated successfully"
        };

        fixture.MockGameService
            .Setup(s => s.UpdateAsync(tournamentId, gameId, gameEditDto))
            .ReturnsAsync(successResponse);

        // Act
        var result = await fixture.Controller.PutGame(tournamentId, gameId, gameEditDto);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
        fixture.MockGameService.Verify(s => s.UpdateAsync(tournamentId, gameId, gameEditDto), Times.Once);
    }

    [Fact]
    public async Task PutGame_OnInvalidIdOrTournamentMismatch_ReturnsBadRequest()
    {
        // Arrange
        var tournamentId = 1;
        var gameId = 1;
        var gameEditDto = TournamentData.GetGameEditDto(tournamentId, 2); // Mismatched game ID
        var errorResponse = new ApiResponse<object>
        {
            Success = false,
            Status = StatusCodes.Status400BadRequest,
            Message = "Game ID mismatch or game does not belong to this tournament"
        };

        fixture.MockGameService
            .Setup(s => s.UpdateAsync(tournamentId, gameId, gameEditDto))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await fixture.Controller.PutGame(tournamentId, gameId, gameEditDto);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusCodeResult.StatusCode);
        fixture.MockGameService.Verify(s => s.UpdateAsync(tournamentId, gameId, gameEditDto), Times.Once);
    }

    [Fact]
    public async Task PostGames_OnSuccessfulCreation_ReturnsCreatedAtAction()
    {
        // Arrange
        var tournamentId = 1;
        var gameCreateDto = TournamentData.GetGameCreateDto(tournamentId);
        var createdGameDto = TournamentData.GetGameDto(tournamentId, 10);
        var successResponse = new ApiResponse<GameDto>
        {
            Success = true,
            Status = StatusCodes.Status201Created,
            Data = createdGameDto,
            Message = "Game created successfully",
            MetaData = 10
        };

        fixture.MockGameService
            .Setup(s => s.CreateAsync(tournamentId, gameCreateDto))
            .ReturnsAsync(successResponse);

        // Act
        var result = await fixture.Controller.PostGames(tournamentId, gameCreateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Game>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        Assert.Equal(nameof(GamesController.GetGames), createdAtActionResult.ActionName);
        Assert.Equal(tournamentId, createdAtActionResult.RouteValues?["tournamentId"]);
        Assert.Equal(10, createdAtActionResult.RouteValues?["id"]);
        Assert.Equal(createdGameDto, createdAtActionResult.Value);
        fixture.MockGameService.Verify(s => s.CreateAsync(tournamentId, gameCreateDto), Times.Once);
    }

    [Fact]
    public async Task PostGames_WhenInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var tournamentId = 1;
        var gameCreateDto = TournamentData.GetGameCreateDto(tournamentId, string.Empty); // Invalid title
        var errorResponse = new ApiResponse<GameDto>
        {
            Success = false,
            Status = StatusCodes.Status400BadRequest,
            Message = "Validation failed",
            Errors = new List<string> { "The Title field is required." }
        };

        fixture.MockGameService
            .Setup(s => s.CreateAsync(tournamentId, gameCreateDto))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await fixture.Controller.PostGames(tournamentId, gameCreateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Game>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusCodeResult.StatusCode);
        fixture.MockGameService.Verify(s => s.CreateAsync(tournamentId, gameCreateDto), Times.Once);
    }

    [Fact]
    public async Task PatchGame_OnSuccessfulPatch_ReturnsOkResult()
    {
        // Arrange
        var tournamentId = 1;
        var gameId = 1;
        var patchDoc = TournamentData.GetGameJsonPatchDocument(gameId, "New Patched Game Title");
        var successResponse = new ApiResponse<GameDto>
        {
            Success = true,
            Status = StatusCodes.Status200OK,
            Data = TournamentData.GetGameDto(tournamentId, gameId),
            Message = "Game patched successfully"
        };

        fixture.MockGameService
            .Setup(s => s.UpdateAsync(tournamentId, gameId, patchDoc))
            .ReturnsAsync(successResponse);

        // Act
        var result = await fixture.Controller.PatchGame(tournamentId, gameId, patchDoc);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        fixture.MockGameService.Verify(s => s.UpdateAsync(tournamentId, gameId, patchDoc), Times.Once);
    }

    [Fact]
    public async Task PatchGame_WhenGameDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var tournamentId = 1;
        var gameId = 999;
        var patchDoc = TournamentData.GetGameJsonPatchDocument(gameId, "NonExistent Game Title");
        var errorResponse = new ApiResponse<GameDto>
        {
            Success = false,
            Status = StatusCodes.Status404NotFound,
            Message = $"Game with id '{gameId}' was not found"
        };

        fixture.MockGameService
            .Setup(s => s.UpdateAsync(tournamentId, gameId, patchDoc))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await fixture.Controller.PatchGame(tournamentId, gameId, patchDoc);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
        fixture.MockGameService.Verify(s => s.UpdateAsync(tournamentId, gameId, patchDoc), Times.Once);
    }

    [Fact]
    public async Task DeleteGames_OnSuccessfulDelete_ReturnsNoContent()
    {
        // Arrange
        var tournamentId = 1;
        var gameId = 1;
        var successResponse = new ApiResponse<object>
        {
            Success = true,
            Status = StatusCodes.Status204NoContent,
            Message = "Game deleted successfully"
        };

        fixture.MockGameService
            .Setup(s => s.DeleteAsync(tournamentId, gameId))
            .ReturnsAsync(successResponse);

        // Act
        var result = await fixture.Controller.DeleteGames(tournamentId, gameId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
        fixture.MockGameService.Verify(s => s.DeleteAsync(tournamentId, gameId), Times.Once);
    }

    [Fact]
    public async Task DeleteGames_WhenGameDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var tournamentId = 1;
        var gameId = 999;
        var errorResponse = new ApiResponse<object>
        {
            Success = false,
            Status = StatusCodes.Status404NotFound,
            Message = $"Game with id '{gameId}' was not found"
        };

        fixture.MockGameService
            .Setup(s => s.DeleteAsync(tournamentId, gameId))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await fixture.Controller.DeleteGames(tournamentId, gameId);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
        fixture.MockGameService.Verify(s => s.DeleteAsync(tournamentId, gameId), Times.Once);
    }
}