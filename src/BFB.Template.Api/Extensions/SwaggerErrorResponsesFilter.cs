using System.Net;
using Abstractions.DTO;
using Abstractions.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BFB.Template.Api.Extensions;

/// <summary>
/// Swagger operation filter to add error response examples
/// </summary>
public class SwaggerErrorResponsesFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Add 400 Bad Request response example
        var badRequestExample = new ErrorResponse
        {
            Status = (int)HttpStatusCode.BadRequest,
            Title = "Bad Request",
            Detail = "The request contains invalid parameters",
            Path = "/api/example",
            Timestamp = DateTime.UtcNow,
            ErrorCode = "BUSINESS_VALIDATION_ERROR",
            RequestId = Guid.NewGuid().ToString(),
            Errors = new Dictionary<string, List<string>>
            {
                { "Property1", new List<string> { "Error message 1", "Error message 2" } },
                { "Property2", new List<string> { "Error message 3" } }
            }
        };

        // Add 404 Not Found response example
        var notFoundExample = new ErrorResponse
        {
            Status = (int)HttpStatusCode.NotFound,
            Title = "Resource Not Found",
            Detail = "The requested resource could not be found",
            Path = "/api/example/123",
            Timestamp = DateTime.UtcNow,
            ErrorCode = "RESOURCE_NOT_FOUND",
            RequestId = Guid.NewGuid().ToString()
        };

        // Add 500 Internal Server Error response example
        var internalServerErrorExample = new ErrorResponse
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred while processing your request",
            Path = "/api/example",
            Timestamp = DateTime.UtcNow,
            ErrorCode = "DATA_ACCESS_ERROR",
            RequestId = Guid.NewGuid().ToString()
        };

        // Set up the responses
        if (operation.Responses == null)
            operation.Responses = new OpenApiResponses();
            
        // Add 400 Bad Request response
        if (!operation.Responses.ContainsKey("400"))
        {
            operation.Responses.Add("400", new OpenApiResponse
            {
                Description = "Bad Request - The request contains invalid parameters",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = context.SchemaGenerator.GenerateSchema(typeof(ErrorResponse), context.SchemaRepository)
                    }
                }
            });
        }
        
        // Add 404 Not Found response
        if (!operation.Responses.ContainsKey("404"))
        {
            operation.Responses.Add("404", new OpenApiResponse
            {
                Description = "Not Found - The requested resource could not be found",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = context.SchemaGenerator.GenerateSchema(typeof(ErrorResponse), context.SchemaRepository)
                    }
                }
            });
        }
        
        // Add 500 Internal Server Error response
        if (!operation.Responses.ContainsKey("500"))
        {
            operation.Responses.Add("500", new OpenApiResponse
            {
                Description = "Internal Server Error - An unexpected error occurred while processing your request",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = context.SchemaGenerator.GenerateSchema(typeof(ErrorResponse), context.SchemaRepository)
                    }
                }
            });
        }
    }
}
