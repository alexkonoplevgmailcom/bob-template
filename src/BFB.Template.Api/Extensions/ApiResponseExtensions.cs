using Abstractions.DTO;
using Abstractions.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BFB.Template.Api.Extensions;

/// <summary>
/// Extension methods for API Controllers to standardize error responses
/// </summary>
public static class ApiResponseExtensions
{
    /// <summary>
    /// Creates a standardized NotFound response
    /// </summary>
    public static ActionResult CreateNotFoundResponse(this ControllerBase controller, string message)
    {
        var errorResponse = new ErrorResponse
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Resource Not Found",
            Detail = message,
            Path = controller.Request.Path,
            RequestId = Activity.Current?.Id ?? controller.HttpContext.TraceIdentifier
        };

        return controller.NotFound(errorResponse);
    }

    /// <summary>
    /// Creates a standardized BadRequest response
    /// </summary>
    public static ActionResult CreateBadRequestResponse(this ControllerBase controller, string message)
    {
        var errorResponse = new ErrorResponse
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = message,
            Path = controller.Request.Path,
            RequestId = Activity.Current?.Id ?? controller.HttpContext.TraceIdentifier
        };

        if (!controller.ModelState.IsValid)
        {
            errorResponse.Errors = controller.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList() ?? new List<string>()
                );
        }

        return controller.BadRequest(errorResponse);
    }

    /// <summary>
    /// Creates a standardized InternalServerError response
    /// </summary>
    public static ActionResult CreateInternalServerErrorResponse(
        this ControllerBase controller, 
        string message, 
        Exception? exception = null)
    {
        var logger = controller.HttpContext.RequestServices.GetRequiredService<ILogger<ControllerBase>>();
        
        if (exception != null)
        {
            logger.LogError(exception, message);
        }
        
        var errorResponse = new ErrorResponse
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = message,
            Path = controller.Request.Path,
            RequestId = Activity.Current?.Id ?? controller.HttpContext.TraceIdentifier
        };

        return controller.StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
    }

    /// <summary>
    /// Creates a response from a custom banking exception
    /// </summary>
    public static ActionResult CreateErrorResponseFromException(this ControllerBase controller, Exception exception)
    {
        var errorResponse = new ErrorResponse
        {
            Path = controller.Request.Path,
            RequestId = Activity.Current?.Id ?? controller.HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        return exception switch
        {
            ResourceNotFoundException ex => controller.CreateResourceNotFoundResponse(ex, errorResponse),
            BusinessValidationException ex => controller.CreateBusinessValidationResponse(ex, errorResponse),
            DataAccessException ex => controller.CreateDataAccessErrorResponse(ex, errorResponse),
            _ => controller.CreateInternalServerErrorResponse("An unexpected error occurred", exception)
        };
    }

    /// <summary>
    /// Creates a response for a resource not found exception
    /// </summary>
    private static ActionResult CreateResourceNotFoundResponse(this ControllerBase controller, ResourceNotFoundException exception, ErrorResponse errorResponse)
    {
        errorResponse.Status = StatusCodes.Status404NotFound;
        errorResponse.Title = "Resource Not Found";
        errorResponse.Detail = exception.Message;
        errorResponse.ErrorCode = exception.ErrorCode;
        
        return controller.NotFound(errorResponse);
    }

    /// <summary>
    /// Creates a response for a business validation exception
    /// </summary>
    private static ActionResult CreateBusinessValidationResponse(this ControllerBase controller, BusinessValidationException exception, ErrorResponse errorResponse)
    {
        errorResponse.Status = StatusCodes.Status400BadRequest;
        errorResponse.Title = "Validation Error";
        errorResponse.Detail = exception.Message;
        errorResponse.ErrorCode = exception.ErrorCode;
        
        return controller.BadRequest(errorResponse);
    }

    /// <summary>
    /// Creates a response for a data access exception
    /// </summary>
    private static ActionResult CreateDataAccessErrorResponse(this ControllerBase controller, DataAccessException exception, ErrorResponse errorResponse)
    {
        var logger = controller.HttpContext.RequestServices.GetRequiredService<ILogger<ControllerBase>>();
        logger.LogError(exception, "Data access error");
        
        errorResponse.Status = StatusCodes.Status503ServiceUnavailable;
        errorResponse.Title = "Service Unavailable";
        errorResponse.Detail = "The service is temporarily unavailable. Please try again later.";
        errorResponse.ErrorCode = exception.ErrorCode;
        
        return controller.StatusCode(StatusCodes.Status503ServiceUnavailable, errorResponse);
    }
}
