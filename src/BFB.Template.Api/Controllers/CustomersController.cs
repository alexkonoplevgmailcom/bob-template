using Abstractions.DTO;
using Abstractions.Interfaces;
using BFB.Template.Api.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace BFB.Template.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetAllCustomers()
    {
        try
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse("Error retrieving all customers", ex);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> GetCustomerById(int id)
    {
        try
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return this.CreateNotFoundResponse($"Customer with ID {id} not found");
            }
            return Ok(customer);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse($"Error retrieving customer {id}", ex);
        }
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
    {
        try
        {
            var createdCustomer = await _customerService.CreateCustomerAsync(customer);
            return CreatedAtAction(nameof(GetCustomerById), new { id = createdCustomer.Id }, createdCustomer);
        }
        catch (ArgumentException ex)
        {
            return this.CreateBadRequestResponse(ex.Message);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse("Error creating customer", ex);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, Customer customer)
    {
        try
        {
            if (id != customer.Id)
            {
                return this.CreateBadRequestResponse("ID in route does not match ID in customer object");
            }

            var success = await _customerService.UpdateCustomerAsync(id, customer);
            if (!success)
            {
                return this.CreateNotFoundResponse($"Customer with ID {id} not found");
            }
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return this.CreateBadRequestResponse(ex.Message);
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse($"Error updating customer {id}", ex);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        try
        {
            var success = await _customerService.DeleteCustomerAsync(id);
            if (!success)
            {
                return this.CreateNotFoundResponse($"Customer with ID {id} not found");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return this.CreateInternalServerErrorResponse($"Error deleting customer {id}", ex);
        }
    }
}