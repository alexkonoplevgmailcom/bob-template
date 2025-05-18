using Abstractions.DTO;
using Abstractions.Exceptions;
using BFB.Template.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BFB.Template.Api.Tests;

/// <summary>
/// Tests for error handling extensions
/// </summary>
public class ApiResponseExtensionsTests
{
    private readonly ILogger<ApiResponseExtensionsTests> _logger;
    private readonly TestController _controller;
    
    public ApiResponseExtensionsTests()
    {
        _logger = new LoggerFactory().CreateLogger<ApiResponseExtensionsTests>();
        _controller = new TestController();
    }
    
    public void RunAllTests()
    {
        Console.WriteLine("Running ApiResponseExtensions Tests");
        
        TestNotFoundResponse();
        TestBadRequestResponse();
        TestInternalServerErrorResponse();
        TestErrorResponseFromException();
        
        Console.WriteLine("All tests completed successfully");
    }
    
    private void TestNotFoundResponse()
    {
        var result = _controller.TestNotFound();
        var notFoundResult = result as NotFoundObjectResult;
        
        // Validate result
        if (notFoundResult == null)
        {
            throw new Exception("NotFoundObjectResult expected");
        }
        
        var errorResponse = notFoundResult.Value as ErrorResponse;
        
        if (errorResponse == null)
        {
            throw new Exception("ErrorResponse expected");
        }
        
        if (errorResponse.Status != 404 || 
            errorResponse.Title != "Resource Not Found" || 
            errorResponse.Detail != "Resource not found test")
        {
            throw new Exception("ErrorResponse properties incorrect");
        }
        
        Console.WriteLine("TestNotFoundResponse: PASSED");
    }
    
    private void TestBadRequestResponse()
    {
        var result = _controller.TestBadRequest();
        var badRequestResult = result as BadRequestObjectResult;
        
        // Validate result
        if (badRequestResult == null)
        {
            throw new Exception("BadRequestObjectResult expected");
        }
        
        var errorResponse = badRequestResult.Value as ErrorResponse;
        
        if (errorResponse == null)
        {
            throw new Exception("ErrorResponse expected");
        }
        
        if (errorResponse.Status != 400 || 
            errorResponse.Title != "Bad Request" || 
            errorResponse.Detail != "Bad request test")
        {
            throw new Exception("ErrorResponse properties incorrect");
        }
        
        Console.WriteLine("TestBadRequestResponse: PASSED");
    }
    
    private void TestInternalServerErrorResponse()
    {
        var result = _controller.TestInternalServerError();
        var objectResult = result as ObjectResult;
        
        // Validate result
        if (objectResult == null || objectResult.StatusCode != 500)
        {
            throw new Exception("ObjectResult with status 500 expected");
        }
        
        var errorResponse = objectResult.Value as ErrorResponse;
        
        if (errorResponse == null)
        {
            throw new Exception("ErrorResponse expected");
        }
        
        if (errorResponse.Status != 500 || 
            errorResponse.Title != "Internal Server Error" || 
            errorResponse.Detail != "Internal server error test")
        {
            throw new Exception("ErrorResponse properties incorrect");
        }
        
        Console.WriteLine("TestInternalServerErrorResponse: PASSED");
    }
    
    private void TestErrorResponseFromException()
    {
        // Test ResourceNotFoundException
        var resourceNotFoundEx = new ResourceNotFoundException("Customer", 123);
        var notFoundResult = _controller.TestException(resourceNotFoundEx) as NotFoundObjectResult;
        
        if (notFoundResult == null)
        {
            throw new Exception("NotFoundObjectResult expected for ResourceNotFoundException");
        }
        
        // Test BusinessValidationException
        var businessValidationEx = new BusinessValidationException("Validation error test");
        var badRequestResult = _controller.TestException(businessValidationEx) as BadRequestObjectResult;
        
        if (badRequestResult == null)
        {
            throw new Exception("BadRequestObjectResult expected for BusinessValidationException");
        }
        
        // Test DataAccessException
        var dataAccessEx = new DataAccessException("Data access error test");
        var serverErrorResult = _controller.TestException(dataAccessEx) as ObjectResult;
        
        if (serverErrorResult == null || serverErrorResult.StatusCode != 500)
        {
            throw new Exception("ObjectResult with status 500 expected for DataAccessException");
        }
        
        Console.WriteLine("TestErrorResponseFromException: PASSED");
    }
    
    /// <summary>
    /// Test controller for testing API response extensions
    /// </summary>
    private class TestController : ControllerBase
    {
        public IActionResult TestNotFound()
        {
            return this.CreateNotFoundResponse("Resource not found test");
        }
        
        public IActionResult TestBadRequest()
        {
            return this.CreateBadRequestResponse("Bad request test");
        }
        
        public IActionResult TestInternalServerError()
        {
            return this.CreateInternalServerErrorResponse("Internal server error test");
        }
        
        public IActionResult TestException(Exception exception)
        {
            return this.CreateErrorResponseFromException(exception);
        }
    }
}
