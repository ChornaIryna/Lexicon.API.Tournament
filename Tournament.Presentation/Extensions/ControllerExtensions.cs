using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tournament.Shared.Responses;

namespace Tournament.Presentation.Extensions;
public static class ControllerExtensions
{
    public static ActionResult HandleApiResponse<T>(this ControllerBase controller, ApiResponse<T> response)
    {
        if (response.MetaData != null)
            controller.Response.Headers.Append("X-Metadata", System.Text.Json.JsonSerializer.Serialize(response.MetaData));

        if (response.Success)
            return response.Data == null
                ? controller.NoContent()
                : controller.Ok(response.Data);

        if (!response.Success || response.Errors.Any())
        {
            var statusCode = response.Status switch
            {
                400 => StatusCodes.Status400BadRequest,
                404 => StatusCodes.Status404NotFound,
                401 => StatusCodes.Status401Unauthorized,
                403 => StatusCodes.Status403Forbidden,
                422 => StatusCodes.Status422UnprocessableEntity,
                500 => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError // Default to 500 if no specific status is set
            };
            controller.Response.StatusCode = statusCode;

            var errorResponse = new ApiError
            {
                Title = "An error occurred",
                Detail = response.Message,
                Status = statusCode,
                Errors = new Dictionary<string, string[]>
                {
                    { "Errors", response.Errors.ToArray() }
                }
            };
            return controller.StatusCode(statusCode, errorResponse);
        }

        return controller.StatusCode(StatusCodes.Status500InternalServerError, response);
    }
}
