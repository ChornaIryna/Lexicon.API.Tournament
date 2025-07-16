using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tournament.Presentation.Controllers;
using Tournament.Shared.DTOs;
using Tournament.Shared.Responses;
using Tournament.Tests.Data;
using Tournament.Tests.TestFixtures;

namespace Tournament.Tests.ControllersTests;
public class TournamentsControllerTests : IClassFixture<TournamentControllerFixture>
{
    private readonly TournamentControllerFixture fixture;

    public TournamentsControllerTests(TournamentControllerFixture fixture)
    {
        this.fixture = fixture;
        fixture.Clear();
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

        fixture.MockTournamentService.Setup(s => s.GetAllAsync(queryParameters, false))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await fixture.Controller.GetTournaments(queryParameters, false);

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<TournamentDto>>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
        fixture.MockTournamentService.Verify(s => s.GetAllAsync(queryParameters, false), Times.Once);
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

        fixture.MockTournamentService.Setup(s => s.GetAllAsync(queryParameters, false))
            .ReturnsAsync(successResponse);

        // Act
        var result = await fixture.Controller.GetTournaments(queryParameters, false);

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<TournamentDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<TournamentDto>>(okResult.Value);
        Assert.Equal(3, Enumerable.Count(returnValue));
        fixture.MockTournamentService.Verify(s => s.GetAllAsync(queryParameters, false), Times.Once);
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

        fixture.MockTournamentService.Setup(s => s.GetByIdAsync(tournamentId, false))
            .ReturnsAsync(successResponse);

        // Act
        var result = await fixture.Controller.GetTournamentDetails(tournamentId, false);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TournamentDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnValue = Assert.IsType<TournamentDto>(okResult.Value);
        Assert.Equal(tournamentDto.Title, returnValue.Title);
        fixture.MockTournamentService.Verify(s => s.GetByIdAsync(tournamentId, false), Times.Once);

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

        fixture.MockTournamentService.Setup(s => s.GetByIdAsync(tournamentId, false))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await fixture.Controller.GetTournamentDetails(tournamentId, false);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TournamentDto>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
        fixture.MockTournamentService.Verify(s => s.GetByIdAsync(tournamentId, false), Times.Once);
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

        fixture.MockTournamentService.Setup(s => s.UpdateAsync(tournamentId, tournamentEditDto))
            .ReturnsAsync(successResponse);

        // Act
        var result = await fixture.Controller.PutTournamentDetails(tournamentId, tournamentEditDto);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
        fixture.MockTournamentService.Verify(s => s.UpdateAsync(tournamentId, tournamentEditDto), Times.Once);
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

        fixture.MockTournamentService.Setup(s => s.UpdateAsync(tournamentId, tournamentEditDto))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await fixture.Controller.PutTournamentDetails(tournamentId, tournamentEditDto);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusCodeResult.StatusCode);
        fixture.MockTournamentService.Verify(s => s.UpdateAsync(tournamentId, tournamentEditDto), Times.Once);
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

        fixture.MockTournamentService.Setup(s => s.CreateAsync(tournamentCreateDto))
            .ReturnsAsync(successResponse);

        // Act
        var result = await fixture.Controller.PostTournamentDetails(tournamentCreateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TournamentDto>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        Assert.Equal(nameof(TournamentsController.GetTournaments), createdAtActionResult.ActionName);
        Assert.Equal(createdTournamentDto, createdAtActionResult.Value);
        fixture.MockTournamentService.Verify(s => s.CreateAsync(tournamentCreateDto), Times.Once);

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

        fixture.MockTournamentService.Setup(s => s.CreateAsync(tournamentCreateDto))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await fixture.Controller.PostTournamentDetails(tournamentCreateDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TournamentDto>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, statusCodeResult.StatusCode);
        fixture.MockTournamentService.Verify(s => s.CreateAsync(tournamentCreateDto), Times.Once);

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

        fixture.MockTournamentService.Setup(s => s.DeleteAsync(tournamentId))
            .ReturnsAsync(successResponse);

        // Act
        var result = await fixture.Controller.DeleteTournamentDetails(tournamentId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, noContentResult.StatusCode);
        fixture.MockTournamentService.Verify(s => s.DeleteAsync(tournamentId), Times.Once);
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

        fixture.MockTournamentService.Setup(s => s.DeleteAsync(tournamentId))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await fixture.Controller.DeleteTournamentDetails(tournamentId);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
        fixture.MockTournamentService.Verify(s => s.DeleteAsync(tournamentId), Times.Once);
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

        fixture.MockTournamentService.Setup(s => s.UpdateAsync(tournamentId, patchDoc))
            .ReturnsAsync(successResponse);

        // Act
        var result = await fixture.Controller.PatchTournamentDetails(tournamentId, patchDoc);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okObjectResult.StatusCode);
        fixture.MockTournamentService.Verify(s => s.UpdateAsync(tournamentId, patchDoc), Times.Once);
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

        fixture.MockTournamentService.Setup(s => s.UpdateAsync(tournamentId, patchDoc))
            .ReturnsAsync(errorResponse);

        // Act
        var result = await fixture.Controller.PatchTournamentDetails(tournamentId, patchDoc);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);
        fixture.MockTournamentService.Verify(s => s.UpdateAsync(tournamentId, patchDoc), Times.Once);
    }
}
