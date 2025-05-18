using System.Net;
using Abstractions.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BFB.Template.Api.Middleware;

/// <summary>
/// Filter to handle model state validation and return standardized error responses
/// </summary>
public class ValidationModelHandlerAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // If the model state is not valid, return a BadRequest with the validation errors
        if (!context.ModelState.IsValid)
        {
            var errors = new Dictionary<string, List<string>>();
            
            foreach (var key in context.ModelState.Keys)
            {
                if (context.ModelState[key]?.Errors.Count > 0)
                {
                    errors[key] = context.ModelState[key]!.Errors
                        .Select(e => e.ErrorMessage)
                        .ToList();
                }
            }

            var errorResponse = new ErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Validation Error",
                Detail = "One or more validation errors occurred.",
                Path = context.HttpContext.Request.Path,
                Timestamp = DateTime.UtcNow,
                Errors = errors
            };

            context.Result = new BadRequestObjectResult(errorResponse);
        }
    }
}
