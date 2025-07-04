using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Tournament.Shared.DTOs;
using Tournament.Shared.Responses;

namespace Tournament.Services;
public abstract class ServiceBase()
{
    protected static ApiResponse<T> CreateErrorResponse<T>(int status, string message, IEnumerable<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Status = status,
            Message = message,
            Errors = errors ?? Enumerable.Empty<string>()
        };
    }

    protected static ApiResponse<T> CreateSuccessResponse<T>(T? data, int status, string message, object? metaData = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Status = status,
            Message = message,
            Data = data,
            MetaData = metaData
        };
    }

    protected static bool ValidateEntity<T>(T entity, out IEnumerable<string> errors)
    {
        var validationContext = new ValidationContext(entity);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(entity, validationContext, validationResults, true);
        errors = validationResults.Select(x => x.ErrorMessage ?? string.Empty);
        return isValid;
    }

    protected static bool ValidateQueryParameters(QueryParameters queryParameters, out ApiResponse<object>? errorResponse)
    {
        errorResponse = null;

        if (queryParameters.PageNumber < 1 || queryParameters.PageSize < 1)
        {
            errorResponse = CreateErrorResponse<object>(
                StatusCodes.Status400BadRequest,
                "Page number and page size must be greater than 0");
            return false;
        }

        if (queryParameters.PageSize > 100)
        {
            errorResponse = CreateErrorResponse<object>(
                StatusCodes.Status400BadRequest,
                "Page size cannot exceed 100");
            return false;
        }

        return true;
    }

    protected object CreatePaginationMetadata(int totalCount, QueryParameters queryParameters)
    {
        return new
        {
            TotalCount = totalCount,
            CurrentPage = queryParameters.PageNumber,
            NumberOfEntitiesOnPage = queryParameters.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)queryParameters.PageSize)
        };
    }
}
