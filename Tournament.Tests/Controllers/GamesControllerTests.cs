using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tournament.Api.Controllers;
using Tournament.Core.DTOs;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;

namespace Tournament.Tests.Controllers;

public class GamesControllerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUoW> _uowMock;
    private readonly Mock<IGameRepository> _gameRepoMock;
    private readonly Mock<ITournamentRepository> _tournamentRepoMock;
    private readonly GamesController _controller;

    public GamesControllerTests()
    {
        _mapperMock = new Mock<IMapper>();
        _uowMock = new Mock<IUoW>();
        _gameRepoMock = new Mock<IGameRepository>();
        _tournamentRepoMock = new Mock<ITournamentRepository>();

        _uowMock.Setup(u => u.GameRepository).Returns(_gameRepoMock.Object);
        _uowMock.Setup(u => u.TournamentRepository).Returns(_tournamentRepoMock.Object);

        _controller = new GamesController(_mapperMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task GetGames_WhenTournamentNotFound_ReturnsNotFound()
    {
        // Arrange
        var tournamentId = 1;
        var queryParams = new QueryParameters();
        _tournamentRepoMock.Setup(r => r.AnyAsync(tournamentId)).ReturnsAsync(false);

        // Act
        var result = await _controller.GetGames(tournamentId, queryParams);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal($"Tournament with Id '{tournamentId}' was not found.", notFoundResult.Value);
    }

    [Fact]
    public async Task GetGames_WhenGamesExist_ReturnsOkWithGames()
    {
        // Arrange
        var tournamentId = 1;
        var queryParams = new QueryParameters();
        var games = new List<Game>
        {
            new() { Id = 1, Title = "Game 1", Time = DateTime.Now },
            new() { Id = 2, Title = "Game 2", Time = DateTime.Now }
        };
        var gameDtos = new List<GameDto>
        {
            new(){Title = "Game 1", Time = DateTime.Now},
            new(){Title = "Game 2", Time = DateTime.Now}
        };

        _tournamentRepoMock.Setup(r => r.AnyAsync(tournamentId)).ReturnsAsync(true);
        _gameRepoMock.Setup(r => r.SearchGamesByTitleAsync(tournamentId, queryParams.SearchTerm))
            .ReturnsAsync(games);
        _mapperMock.Setup(m => m.Map<IEnumerable<GameDto>>(It.IsAny<IEnumerable<Game>>()))
            .Returns(gameDtos);

        // Act
        var result = await _controller.GetGames(tournamentId, queryParams);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedGames = Assert.IsAssignableFrom<IEnumerable<GameDto>>(okResult.Value);
        Assert.Equal(2, returnedGames.Count());
    }

    [Fact]
    public async Task PutGame_WhenValidUpdate_ReturnsNoContent()
    {
        // Arrange
        var tournamentId = 1;
        var gameId = 1;
        var gameEditDto = new GameEditDto() { Id = gameId, Title = "Updated Game", Time = DateTime.Now };
        var existingGame = new Game { Id = gameId, Title = "Original Game", TournamentDetailsId = tournamentId };

        _tournamentRepoMock.Setup(r => r.AnyAsync(tournamentId)).ReturnsAsync(true);
        _gameRepoMock.Setup(r => r.FindByIdAsync(gameId, true)).ReturnsAsync(existingGame);
        _uowMock.Setup(u => u.HasChanges()).Returns(true);

        // Act
        var result = await _controller.PutGame(tournamentId, gameId, gameEditDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteGame_WhenGameExists_ReturnsNoContent()
    {
        // Arrange
        var tournamentId = 1;
        var gameId = 1;
        var game = new Game { Id = gameId, Title = "Existing Game", TournamentDetailsId = tournamentId };

        _tournamentRepoMock.Setup(r => r.AnyAsync(tournamentId)).ReturnsAsync(true);
        _gameRepoMock.Setup(r => r.FindByIdAsync(gameId, It.IsAny<bool>())).ReturnsAsync(game);

        // Act
        var result = await _controller.DeleteGames(tournamentId, gameId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task PostGame_WhenValidGame_ReturnsCreatedAtAction()
    {
        // Arrange
        var tournamentId = 1;
        var gameCreateDto = new GameCreateDto() { Title = "New Game", Time = DateTime.Now };
        var game = new Game { Id = 1, Title = "New Game", Time = DateTime.Now };
        var gameDto = new GameDto() { Title = "New Game", Time = DateTime.Now };

        _tournamentRepoMock.Setup(r => r.AnyAsync(tournamentId)).ReturnsAsync(true);
        _mapperMock.Setup(m => m.Map<Game>(gameCreateDto)).Returns(game);
        _mapperMock.Setup(m => m.Map<GameDto>(game)).Returns(gameDto);

        // Act
        var result = await _controller.PostGames(tournamentId, gameCreateDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(GamesController.GetGames), createdAtActionResult.ActionName);
    }
}