using Abstractions.DTO;
using Abstractions.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace BFB.Template.Api.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        // Set appropriate status code based on exception type
        context.Response.StatusCode = exception switch
        {
            ResourceNotFoundException => StatusCodes.Status404NotFound,
            BusinessValidationException => StatusCodes.Status400BadRequest,
            DataAccessException => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };

        var errorResponse = new ErrorResponse
        {
            Status = context.Response.StatusCode,
            Title = exception switch
            {
                ResourceNotFoundException => "Resource Not Found",
                BusinessValidationException => "Validation Error",
                DataAccessException => "Data Access Error",
                _ => "Internal Server Error"
            },
            Detail = _environment.IsDevelopment() 
                ? exception.Message 
                : exception switch
                {
                    ResourceNotFoundException => exception.Message,
                    BusinessValidationException => exception.Message,
                    _ => "An unexpected error occurred. Please try again later."
                },
            Path = context.Request.Path,
            RequestId = Activity.Current?.Id ?? context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };
        
        // Set error code for banking exceptions
        if (exception is BankingException bankingException)
        {
            errorResponse.ErrorCode = bankingException.ErrorCode;
        }

        // Include stack trace in development environment only
        if (_environment.IsDevelopment())
        {
            errorResponse.Errors = new Dictionary<string, List<string>>
            {
                ["ExceptionDetails"] = new List<string> { exception.ToString() }
            };
        }

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var json = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await context.Response.WriteAsync(json);
    }
}

// Extension method to make it easier to add the middleware to the request pipeline
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
