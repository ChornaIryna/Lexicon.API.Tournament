using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Service.Contracts.Interfaces;
using Tournament.Presentation.Controllers;
using Tournament.Shared.DTOs;
using Tournament.Shared.Responses;
using Tournament.Tests.Helpers;

namespace Tournament.Tests.ControllersTests;
public class TournamentsControllerTests
{
    private readonly Mock<IServiceManager> _mockServiceManager;
    private readonly Mock<ITournamentService> _mockTournamentService;
    private readonly TournamentsController _controller;

    public TournamentsControllerTests()
    {
        _mockServiceManager = new Mock<IServiceManager>();
        _mockTournamentService = new Mock<ITournamentService>();

        _mockServiceManager.Setup(sm => sm.TournamentService).Returns(_mockTournamentService.Object);

        _controller = new TournamentsController(_mockServiceManager.Object);

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task GetTournaments_WhenNoTournaments_ReturnsNotFound()
    {
        // Arrange
        var queryParameters = TournamentData.GetQueryParameters();
        var errorResponse = new ApiResponse<IEnumerable<TournamentDto>>
        {
            Success = false,
            Status = StatusCodes.Status404NotFound,
            Message = "No tournaments found"
        };

        _mockTournamentService.Setup(s => s.GetAllAsync(queryParameters, false))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.GetTournaments(queryParameters, false);

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<TournamentDto>>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
        _mockTournamentService.Verify(s => s.GetAllAsync(queryParameters, false), Times.Once);
    }

    [Fact]
    public async Task GetTournaments_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var queryParameters = TournamentData.GetQueryParameters();
        var tournaments = TournamentData.GetTournamentDtos(3);
        var successResponse = new ApiResponse<IEnumerable<TournamentDto>>
        {
            Success = true,
            Status = StatusCodes.Status200OK,
            Data = tournaments,
            Message = "Tournaments retrieved successfully",
            MetaData = new { TotalCount = 3, CurrentPage = 1, NumberOfEntitiesOnPage = 10, TotalPages = 1 }
        };

        _mockTournamentService.Setup(s => s.GetAllAsync(queryParameters, false))
            .ReturnsAsync(successResponse);

        // Act
        var result = await _controller.GetTournaments(queryParameters, false);

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<TournamentDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<TournamentDto>>(okResult.Value);
        Assert.Equal(3, System.Linq.Enumerable.Count(returnValue));
        _mockTournamentService.Verify(s => s.GetAllAsync(queryParameters, false), Times.Once);
    }

    [Fact]
    public async Task GetTournamentDetails_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var tournamentId = 1;
        var tournamentDto = TournamentData.GetTournamentDto(tournamentId);
        var successResponse = new ApiResponse<TournamentDto>
        {
            Success = true,
            Status = StatusCodes.Status200OK,
            Data = tournamentDto,
            Message = "Tournament retrieved successfully"
        };

        _mockTournamentService.Setup(s => s.GetByIdAsync(tournamentId, false))
            .ReturnsAsync(successResponse);

        // Act
        var result = await _controller.GetTournamentDetails(tournamentId, false);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TournamentDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<TournamentDto>(okResult.Value);
        Assert.Equal(tournamentDto.Title, returnValue.Title);
        _mockTournamentService.Verify(s => s.GetByIdAsync(tournamentId, false), Times.Once);

    }

    [Fact]
    public async Task GetTournamentDetails_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var tournamentId = 999;
        var errorResponse = new ApiResponse<TournamentDto>
        {
            Success = false,
            Status = StatusCodes.Status404NotFound,
            Message = $"Tournament with id '{tournamentId}' was not found"
        };

        _mockTournamentService.Setup(s => s.GetByIdAsync(tournamentId, false))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.GetTournamentDetails(tournamentId, false);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TournamentDto>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
        _mockTournamentService.Verify(s => s.GetByIdAsync(tournamentId, false), Times.Once);
    }

    [Fact]
    public async Task PutTournamentDetails_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var tournamentId = 1;
        var tournamentEditDto = TournamentData.GetTournamentEditDto(tournamentId);
        var successResponse = new ApiResponse<object>
        {
            Success = true,
            Status = StatusCodes.Status204NoContent,
            Message = "Tournament updated successfully"
        };

        _mockTournamentService.Setup(s => s.UpdateAsync(tournamentId, tournamentEditDto))
            .ReturnsAsync(successResponse);

        // Act
        var result = await _controller.PutTournamentDetails(tournamentId, tournamentEditDto);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
        _mockTournamentService.Verify(s => s.UpdateAsync(tournamentId, tournamentEditDto), Times.Once);
    }

    [Fact]
    public async Task PutTournamentDetails_WithInvalidId_ReturnsBadRequest()
    {// Arrange
        var tournamentId = 1;
        var tournamentEditDto = TournamentData.GetTournamentEditDto(2); // Mismatched ID
        var errorResponse = new ApiResponse<object>
        {
            Success = false,
            Status = StatusCodes.Status400BadRequest,
            Message = "Invalid tournament ID"
        };

        _mockTournamentService.Setup(s => s.UpdateAsync(tournamentId, tournamentEditDto))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.PutTournamentDetails(tournamentId, tournamentEditDto);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusCodeResult.StatusCode);
        _mockTournamentService.Verify(s => s.UpdateAsync(tournamentId, tournamentEditDto), Times.Once);
    }

    [Fact]
    public async Task PostTournamentDetails_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var tournamentCreateDto = TournamentData.GetTournamentCreateDto();
        var createdTournamentDto = new TournamentDto
        {
            Title = tournamentCreateDto.Title,
            StartDate = tournamentCreateDto.StartDate,
            EndDate = tournamentCreateDto.StartDate.AddMonths(3)
        };
        var successResponse = new ApiResponse<TournamentDto>
        {
            Success = true,
            Status = StatusCodes.Status201Created,
            Data = createdTournamentDto,
            Message = "Tournament created successfully"
        };

        _mockTournamentService.Setup(s => s.CreateAsync(tournamentCreateDto))
            .ReturnsAsync(successResponse);

        // Act
        var result = await _controller.PostTournamentDetails(tournamentCreateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TournamentDto>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        Assert.Equal(nameof(TournamentsController.GetTournaments), createdAtActionResult.ActionName);
        Assert.Equal(createdTournamentDto, createdAtActionResult.Value);
        _mockTournamentService.Verify(s => s.CreateAsync(tournamentCreateDto), Times.Once);

    }

    [Fact]
    public async Task PostTournamentDetails_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var tournamentCreateDto = TournamentData.GetTournamentCreateDto(string.Empty);//invalid Title
        var errorResponse = new ApiResponse<TournamentDto>
        {
            Success = false,
            Status = StatusCodes.Status400BadRequest,
            Message = "Validation failed",
            Errors = new List<string> { "The Title field is required." }
        };

        _mockTournamentService.Setup(s => s.CreateAsync(tournamentCreateDto))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.PostTournamentDetails(tournamentCreateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TournamentDto>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusCodeResult.StatusCode);
        _mockTournamentService.Verify(s => s.CreateAsync(tournamentCreateDto), Times.Once);

    }


    [Fact]
    public async Task DeleteTournamentDetails_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var tournamentId = 1;
        var successResponse = new ApiResponse<object>
        {
            Success = true,
            Status = StatusCodes.Status204NoContent,
            Message = "Tournament deleted successfully"
        };

        _mockTournamentService.Setup(s => s.DeleteAsync(tournamentId))
            .ReturnsAsync(successResponse);

        // Act
        var result = await _controller.DeleteTournamentDetails(tournamentId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
        _mockTournamentService.Verify(s => s.DeleteAsync(tournamentId), Times.Once);
    }

    [Fact]
    public async Task DeleteTournamentDetails_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var tournamentId = 999;
        var errorResponse = new ApiResponse<object>
        {
            Success = false,
            Status = StatusCodes.Status404NotFound,
            Message = $"Tournament with id '{tournamentId}' was not found"
        };

        _mockTournamentService.Setup(s => s.DeleteAsync(tournamentId))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.DeleteTournamentDetails(tournamentId);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
        _mockTournamentService.Verify(s => s.DeleteAsync(tournamentId), Times.Once);
    }

    [Fact]
    public async Task PatchTournamentDetails_OnSuccessfulPatch_ReturnsNoContent()
    {
        // Arrange
        var tournamentId = 1;
        var patchDoc = TournamentData.GetTournamentJsonPatchDocument(tournamentId, "New Patched Title");
        var successResponse = new ApiResponse<TournamentDto>
        {
            Success = true,
            Status = StatusCodes.Status200OK,
            Data = TournamentData.GetTournamentDto(tournamentId),
            Message = "Tournament updated successfully"
        };

        _mockTournamentService.Setup(s => s.UpdateAsync(tournamentId, patchDoc))
            .ReturnsAsync(successResponse);

        // Act
        var result = await _controller.PatchTournamentDetails(tournamentId, patchDoc);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        _mockTournamentService.Verify(s => s.UpdateAsync(tournamentId, patchDoc), Times.Once);
    }

    [Fact]
    public async Task PatchTournamentDetails_WithInvalidTournamentId_ReturnsNotFound()
    {
        // Arrange
        var tournamentId = 999;
        var patchDoc = TournamentData.GetTournamentJsonPatchDocument(tournamentId, "New Patched Title");
        var errorResponse = new ApiResponse<TournamentDto>
        {
            Success = false,
            Status = StatusCodes.Status404NotFound,
            Message = $"Tournament with id '{tournamentId}' was not found"
        };

        _mockTournamentService.Setup(s => s.UpdateAsync(tournamentId, patchDoc))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.PatchTournamentDetails(tournamentId, patchDoc);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
        _mockTournamentService.Verify(s => s.UpdateAsync(tournamentId, patchDoc), Times.Once);
    }
}
