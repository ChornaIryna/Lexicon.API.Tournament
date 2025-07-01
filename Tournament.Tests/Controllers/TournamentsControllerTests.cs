using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tournament.Api.Controllers;
using Tournament.Core.DTOs;
using Tournament.Core.Entities;
using Tournament.Core.Repositories;

namespace Tournament.Tests.Controllers;
public class TournamentsControllerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUoW> _uowMock;
    private readonly Mock<ITournamentRepository> _tournamentRepoMock;
    private readonly TournamentsController _controller;

    public TournamentsControllerTests()
    {
        _mapperMock = new Mock<IMapper>();
        _uowMock = new Mock<IUoW>();
        _tournamentRepoMock = new Mock<ITournamentRepository>();
        _uowMock.Setup(u => u.TournamentRepository).Returns(_tournamentRepoMock.Object);
        _controller = new TournamentsController(_mapperMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task GetTournaments_WhenNoTournaments_ReturnsNotFound()
    {
        // Arrange
        _tournamentRepoMock.Setup(repo => repo.GetAllAsync(It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new List<TournamentDetails>());

        // Act
        var result = await _controller.GetTournaments(new QueryParameters());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetTournaments_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var tournaments = new List<TournamentDetails>
        {
            new() { Id = 1, Title = "Tournament 1" },
            new() { Id = 2, Title = "Tournament 2" }
        };

        var tournamentDtos = new List<TournamentDto>
        {
            new() { Title = "Tournament 1" },
            new() { Title = "Tournament 2" }
        };

        _tournamentRepoMock.Setup(repo => repo.GetAllAsync(It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(tournaments);
        _mapperMock.Setup(m => m.Map<IEnumerable<TournamentDto>>(It.IsAny<IEnumerable<TournamentDetails>>()))
            .Returns(tournamentDtos);

        // Act
        var result = await _controller.GetTournaments(new QueryParameters());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTournaments = Assert.IsAssignableFrom<IEnumerable<TournamentDto>>(okResult.Value);
        Assert.Equal(2, returnedTournaments.Count());
    }

    [Fact]
    public async Task GetTournamentDetails_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _tournamentRepoMock.Setup(repo => repo.FindByIdAsync(It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync((TournamentDetails?)null);

        // Act
        var result = await _controller.GetTournamentDetails(1);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task PutTournamentDetails_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var tournamentDto = new TournamentEditDto { Id = 1, Title = "Updated Tournament" };
        var tournament = new TournamentDetails { Id = 1, Title = "Tournament" };

        _tournamentRepoMock.Setup(repo => repo.FindByIdAsync(1, true))
            .ReturnsAsync(tournament);
        _tournamentRepoMock.Setup(repo => repo.AnyAsync(1))
            .ReturnsAsync(true);
        _uowMock.Setup(u => u.HasChanges()).Returns(true);

        // Act
        var result = await _controller.PutTournamentDetails(1, tournamentDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task PostTournamentDetails_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new TournamentCreateDto { Title = "New Tournament" };
        var tournament = new TournamentDetails { Id = 1, Title = "New Tournament" };
        var tournamentDto = new TournamentDto { Title = "New Tournament" };

        _mapperMock.Setup(m => m.Map<TournamentDetails>(createDto))
            .Returns(tournament);
        _mapperMock.Setup(m => m.Map<TournamentDto>(tournament))
            .Returns(tournamentDto);

        // Act
        var result = await _controller.PostTournamentDetails(createDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(TournamentsController.GetTournamentDetails), createdAtActionResult.ActionName);
    }

    [Fact]
    public async Task DeleteTournamentDetails_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var tournament = new TournamentDetails { Id = 1, Title = "Tournament" };
        _tournamentRepoMock.Setup(repo => repo.FindByIdAsync(1, false))
            .ReturnsAsync(tournament);

        // Act
        var result = await _controller.DeleteTournamentDetails(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _tournamentRepoMock.Verify(repo => repo.Remove(tournament), Times.Once);
    }

    [Fact]
    public async Task DeleteTournamentDetails_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _tournamentRepoMock.Setup(repo => repo.FindByIdAsync(It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync((TournamentDetails?)null);
        // Act
        var result = await _controller.DeleteTournamentDetails(1);
        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
